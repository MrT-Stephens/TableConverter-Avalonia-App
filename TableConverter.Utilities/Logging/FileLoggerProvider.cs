using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TableConverter.Utilities.Logging;

/// <summary>
/// Generic file logger provider.
/// </summary>
[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{

	private string _LogFileName;
	private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
	private readonly BlockingCollection<string> _entryQueue = new(1024);
	private readonly Task _processQueueTask;
	private readonly FileWriter _fWriter;

	internal FileLoggerOptions Options { get; private set; }

	private bool Append => Options.Append;
	private long FileSizeLimitBytes => Options.FileSizeLimitBytes;
	private int MaxRollingFiles => Options.MaxRollingFiles;

	public LogLevel MinLevel
	{
		get => Options.MinLevel;
		set => Options.MinLevel = value;
	}

	/// <summary>
	///  Gets or sets indication whether UTC timezone should be used to for timestamps in logging messages. Defaults to false.
	/// </summary>
	public bool UseUtcTimestamp
	{
		get => Options.UseUtcTimestamp;
		set => Options.UseUtcTimestamp = value;
	}

	/// <summary>
	/// Custom formatter for log entry. 
	/// </summary>
	public Func<LogMessage, string>? FormatLogEntry
	{
		get => Options.FormatLogEntry;
		set => Options.FormatLogEntry = value;
	}

	/// <summary>
	/// Custom formatter for the log file name.
	/// </summary>
	public Func<string, string>? FormatLogFileName
	{
		get => Options.FormatLogFileName;
		set => Options.FormatLogFileName = value;
	}

	/// <summary>
	/// Custom handler for file errors.
	/// </summary>
	public Action<FileError>? HandleFileError
	{
		get => Options.HandleFileError;
		set => Options.HandleFileError = value;
	}

	public FileLoggerProvider(string fileName) 
		: this(fileName, true)
	{
	}

	public FileLoggerProvider(string fileName, bool append) : this(fileName,
		new FileLoggerOptions() { Append = append })
	{
	}

	public FileLoggerProvider(string fileName, FileLoggerOptions options)
	{
		Options = options;
		_LogFileName = Environment.ExpandEnvironmentVariables(fileName);

		_fWriter = new FileWriter(this);
		_processQueueTask = Task.Factory.StartNew(
			ProcessQueue,
			this,
			TaskCreationOptions.LongRunning);
	}

	public ILogger CreateLogger(string categoryName)
	{
		return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
	}

	public void Dispose()
	{
		_entryQueue.CompleteAdding();
		
		try
		{
			_processQueueTask.Wait(1500); // the same as in ConsoleLogger
		}
		catch (TaskCanceledException)
		{
		}
		catch (AggregateException ex) when (ex.InnerExceptions is [TaskCanceledException])
		{
		}

		_loggers.Clear();
		_fWriter.Close();
	}

	private FileLogger CreateLoggerImplementation(string name)
	{
		return new FileLogger(name, this);
	}

	internal void WriteEntry(string message)
	{
		if (!_entryQueue.IsAddingCompleted)
		{
			try
			{
				_entryQueue.Add(message);
				return;
			}
			catch (InvalidOperationException)
			{
			}
		}
		// do nothing
	}

	private void ProcessQueue()
	{
		var writeMessageFailed = false;
		foreach (var message in _entryQueue.GetConsumingEnumerable())
		{
			try
			{
				if (!writeMessageFailed)
				{
					_fWriter.WriteMessage(message, _entryQueue.Count == 0);
				}
			}
			catch (Exception ex)
			{
				// something goes wrong. App's code can handle it if 'HandleFileError' is provided
				var stopLogging = true;
				
				if (HandleFileError != null)
				{
					var fileErr = new FileError(_LogFileName, ex);
					
					try
					{
						HandleFileError(fileErr);
						
						if (fileErr.NewLogFileName != null)
						{
							_fWriter.UseNewLogFile(fileErr.NewLogFileName);
							// write a failed message to a new log file
							_fWriter.WriteMessage(message, _entryQueue.Count == 0);
							stopLogging = false;
						}
					}
					catch
					{
						// an exception is possible in HandleFileError, or if the proposed file name cannot be used,
						// let's ignore it in that case -> file logger will stop processing log messages
					}
				}

				if (stopLogging)
				{
					// Stop processing log messages since they cannot be written to a log file
					_entryQueue.CompleteAdding();
					writeMessageFailed = true;
				}
			}
		}
	}

	private static void ProcessQueue(object? state)
	{
		var fileLogger = (FileLoggerProvider)state!;
		fileLogger.ProcessQueue();
	}

	private class FileWriter
	{
		private readonly FileLoggerProvider _fileLogPrv;
		private string _LogFileName;
		private int _RollingNumber;
		private Stream? _LogFileStream;
		private TextWriter? _LogFileWriter;

		internal FileWriter(FileLoggerProvider fileLogPrv)
		{
			_fileLogPrv = fileLogPrv;

			DetermineLastFileLogName();
			OpenFile(_fileLogPrv.Append);
		}

		private string GetBaseLogFileName()
		{
			var fName = _fileLogPrv._LogFileName;

			if (_fileLogPrv.FormatLogFileName != null)
			{
				fName = _fileLogPrv.FormatLogFileName(fName);
			}

			return fName;
		}

		private void DetermineLastFileLogName()
		{
			var baseLogFileName = GetBaseLogFileName();
			_LastBaseLogFileName = baseLogFileName;
			
			if (_fileLogPrv.FileSizeLimitBytes > 0)
			{
				// rolling file is used
				if (_fileLogPrv.Options.RollingFilesConvention == FileLoggerOptions.FileRollingConvention.Ascending)
				{
					var logFiles = GetExistingLogFiles(baseLogFileName);
					
					if (logFiles.Length > 0)
					{
						var lastFileInfo = logFiles
							.OrderByDescending(fInfo => fInfo.Name)
							.ThenByDescending(fInfo => fInfo.LastWriteTime).First();
						
						_LogFileName = lastFileInfo.FullName;
					}
					else
					{
						// no files yet, use the default name
						_LogFileName = baseLogFileName;
					}
				}
				else
				{
					_LogFileName = baseLogFileName;
				}
			}
			else
			{
				_LogFileName = baseLogFileName;
			}
		}

		private void CreateLogFileStream(bool append)
		{
			var fileInfo = new FileInfo(_LogFileName);
			
			// Directory.Create will check if the directory already exists,
			// so there is no need for a "manual" check first.
			fileInfo.Directory?.Create();

			_LogFileStream = new FileStream(_LogFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
			
			if (append)
			{
				_LogFileStream.Seek(0, SeekOrigin.End);
			}
			else
			{
				_LogFileStream.SetLength(0); // clear the file
			}

			_LogFileWriter = new StreamWriter(_LogFileStream);
		}

		internal void UseNewLogFile(string newLogFileName)
		{
			_fileLogPrv._LogFileName = newLogFileName;
			
			DetermineLastFileLogName(); // preserve all existing logic related to 'FormatLogFileName' and rolling files
			
			CreateLogFileStream(_fileLogPrv.Append); // if file error occurs here it is not handled by 'HandleFileError' recursively
		}

		private void OpenFile(bool append)
		{
			try
			{
				CreateLogFileStream(append);
			}
			catch (Exception ex)
			{
				if (_fileLogPrv.HandleFileError != null)
				{
					var fileErr = new FileError(_LogFileName, ex);
					_fileLogPrv.HandleFileError(fileErr);
					
					if (fileErr.NewLogFileName != null)
					{
						UseNewLogFile(fileErr.NewLogFileName);
					}
				}
				else
				{
					throw; // do not handle by default to preserve backward compatibility
				}
			}
		}


		private string GetNextFileLogName()
		{
			var baseLogFileName = GetBaseLogFileName();
			
			// if a file does not exist or the file size limit is not reached - do not add a rolling file index
			if (!File.Exists(baseLogFileName)
			    || _fileLogPrv.FileSizeLimitBytes <= 0
			    || new FileInfo(baseLogFileName).Length < _fileLogPrv.FileSizeLimitBytes)
			{
				return baseLogFileName;
			}

			switch (_fileLogPrv.Options.RollingFilesConvention)
			{
				case FileLoggerOptions.FileRollingConvention.Ascending:
				{
					//Unchanged default handling just optimized for performance and code reuse
					int currentFileIndex = GetIndexFromFile(baseLogFileName, _LogFileName);
					var nextFileIndex = currentFileIndex + 1;
					
					if (_fileLogPrv.MaxRollingFiles > 0)
					{
						nextFileIndex %= _fileLogPrv.MaxRollingFiles;
					}

					return GetFileFromIndex(baseLogFileName, nextFileIndex);
				}
				case FileLoggerOptions.FileRollingConvention.AscendingStableBase:
				{
					//Move current base file to next rolling file number
					_RollingNumber++;
					
					if (_fileLogPrv.MaxRollingFiles > 0)
					{
						_RollingNumber %= _fileLogPrv.MaxRollingFiles - 1;
					}

					var moveFile = GetFileFromIndex(baseLogFileName, _RollingNumber + 1);
					
					if (File.Exists(moveFile))
					{
						File.Delete(moveFile);
					}

					File.Move(baseLogFileName, moveFile);
					
					return baseLogFileName;
				}
				case FileLoggerOptions.FileRollingConvention.Descending:
				{
					//Move all existing files to index +1 except if they are > MaxRollingFiles
					var logFiles = GetExistingLogFiles(baseLogFileName);
					
					if (logFiles.Length > 0)
					{
						foreach (var fileInfo in logFiles.OrderByDescending(fInfo => fInfo.Name))
						{
							var index = GetIndexFromFile(baseLogFileName, fileInfo.Name);
							
							if (_fileLogPrv.MaxRollingFiles > 0 && index >= _fileLogPrv.MaxRollingFiles - 1)
							{
								continue;
							}

							var moveFile = GetFileFromIndex(baseLogFileName, index + 1);
							
							if (File.Exists(moveFile))
							{
								File.Delete(moveFile);
							}

							File.Move(fileInfo.FullName, moveFile);
						}
					}

					return baseLogFileName;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(_fileLogPrv.Options.RollingFilesConvention));
			}
		}

		// cache last returned base log file name to avoid excessive checks in CheckForNewLogFile.isBaseFileNameChanged
		private string? _LastBaseLogFileName;

		private void CheckForNewLogFile()
		{
			bool openNewFile = IsMaxFileSizeThresholdReached() || IsBaseFileNameChanged();

			if (openNewFile)
			{
				Close();
				_LogFileName = GetNextFileLogName();
				OpenFile(false);
			}

			return;

			bool IsMaxFileSizeThresholdReached()
			{
				return _fileLogPrv.FileSizeLimitBytes > 0 
				    && _LogFileStream != null 
				    && _LogFileStream.Length > _fileLogPrv.FileSizeLimitBytes;
			}

			bool IsBaseFileNameChanged()
			{
				if (_fileLogPrv.FormatLogFileName != null)
				{
					var baseLogFileName = GetBaseLogFileName();
					
					if (baseLogFileName != _LastBaseLogFileName)
					{
						_LastBaseLogFileName = baseLogFileName;
						return true;
					}

					return false;
				}

				return false;
			}
		}

		internal void WriteMessage(string message, bool flush)
		{
			if (_LogFileWriter != null)
			{
				CheckForNewLogFile();
				_LogFileWriter.WriteLine(message);

				if (flush)
				{
					_LogFileWriter.Flush();
				}
			}
		}

		/// <summary>
		/// Returns the index of a file or 0 if none found
		/// </summary>
		private int GetIndexFromFile(string baseLogFileName, string filename)
		{
#if NETSTANDARD2_0
			var baseFileNameOnly = Path.GetFileNameWithoutExtension(baseLogFileName);
			var currentFileNameOnly = Path.GetFileNameWithoutExtension(filename);

			var suffix = currentFileNameOnly.Substring(baseFileNameOnly.Length);
#else
			var baseFileNameOnly = Path.GetFileNameWithoutExtension(baseLogFileName.AsSpan());
			var currentFileNameOnly = Path.GetFileNameWithoutExtension(filename.AsSpan());

			var suffix = currentFileNameOnly.Slice(baseFileNameOnly.Length);
#endif
			if (suffix.Length > 0 && int.TryParse(suffix, out var parsedIndex))
			{
				return parsedIndex;
			}

			return 0;
		}

		private string GetFileFromIndex(string baseLogFileName, int index)
		{
#if NETSTANDARD
			var nextFileName = Path.GetFileNameWithoutExtension(baseLogFileName) 
				+ (index > 0 ? index.ToString() : "") + Path.GetExtension(baseLogFileName);

			return Path.Combine(Path.GetDirectoryName(baseLogFileName), nextFileName);
#else
			// Contact for ReadOnlySpan<char> is not available in both netstandard2.0 and netstandard2.1
			var nextFileName = string.Concat(Path.GetFileNameWithoutExtension(baseLogFileName.AsSpan()),
				index > 0 ? index.ToString() : "", Path.GetExtension(baseLogFileName.AsSpan()));
			
			return string.Concat(Path.Join(Path.GetDirectoryName(baseLogFileName.AsSpan()), nextFileName.AsSpan()));
#endif
		}

		private FileInfo[] GetExistingLogFiles(string baseLogFileName)
		{
			var logFileMask = Path.GetFileNameWithoutExtension(baseLogFileName) 
			    + "*" + Path.GetExtension(baseLogFileName);
			
			var logDirName = Path.GetDirectoryName(baseLogFileName);

			if (string.IsNullOrEmpty(logDirName))
			{
				logDirName = Directory.GetCurrentDirectory();
			}

			var logDirectory = new DirectoryInfo(logDirName);
			
			return logDirectory.Exists
				? logDirectory.GetFiles(logFileMask, SearchOption.TopDirectoryOnly)
				: [];
		}

		internal void Close()
		{
			if (_LogFileWriter != null)
			{
				var logWriter = _LogFileWriter;
				_LogFileWriter = null;

				logWriter.Dispose();
				_LogFileStream?.Dispose();
				_LogFileStream = null;
			}

		}
	}

	/// <summary>
	/// Represents a file error context.
	/// </summary>
	public class FileError
	{

		/// <summary>
		/// Exception that occurs on the file operation.
		/// </summary>
		public Exception ErrorException { get; private set; }

		/// <summary>
		/// Current log file name.
		/// </summary>
		public string LogFileName { get; private set; }

		internal FileError(string logFileName, Exception ex)
		{
			LogFileName = logFileName;
			ErrorException = ex;
		}

		internal string? NewLogFileName { get; private set; }

		/// <summary>
		/// Suggests a new log file name to use instead of the current one. 
		/// </summary>
		/// <remarks>
		/// If the proposed file name also leads to a file error this will break a file logger: errors are not handled recursively.
		/// </remarks>
		/// <param name="newLogFileName">a new log file name</param>
		public void UseNewLogFileName(string newLogFileName)
		{
			NewLogFileName = newLogFileName;
		}
	}
}