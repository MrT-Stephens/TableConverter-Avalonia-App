using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Logging;

namespace TableConverter.Utilities.Tests.Logging;

public class FileProviderTests
{
	[Fact]
	public void WriteToFileAndOverwrite()
	{
		var tmpFile = Path.GetTempFileName();

		try
		{
			var factory = new LoggerFactory();
			factory.AddProvider(new FileLoggerProvider(tmpFile, false));
			var logger = factory.CreateLogger("TEST");
			logger.LogInformation("Line1");
			factory.Dispose();

			Assert.Single(File.ReadAllLines(tmpFile));

			factory = new LoggerFactory();
			logger = factory.CreateLogger("TEST");
			factory.AddProvider(new FileLoggerProvider(tmpFile, false));
			logger.LogInformation("Line2");
			factory.Dispose();

			Assert.Single(File.ReadAllLines(tmpFile)); // file should be overwritten

		}
		finally
		{
			File.Delete(tmpFile);
		}
	}

	[Fact]
	public void WriteToFileAndAppend()
	{
		var tmpFile = Path.GetTempFileName();

		try
		{
			var factory = new LoggerFactory();
			factory.AddProvider(new FileLoggerProvider(tmpFile));

			var logger = factory.CreateLogger("TEST");
			logger.LogDebug("Debug message");

			logger.LogWarning("Warning message");

			factory.Dispose();

			//System.IO.File.WriteAllText("test.txt", System.IO.File.ReadAllText(tmpFile));
			var logEntries = File.ReadAllLines(tmpFile);
			Assert.Equal(2, logEntries.Length);

			var entry1Parts = logEntries[0].Split('\t');
			Assert.Equal("[TEST]", entry1Parts[2]);
			Assert.Equal("Debug message", entry1Parts[4]);
			Assert.True(DateTime.Parse(entry1Parts[0]).Ticks <= DateTime.Now.Ticks);

			var entry2Parts = logEntries[1].Split('\t');
			Assert.Equal("[TEST]", entry2Parts[2]);
			Assert.Equal("Warning message", entry2Parts[4]);

			factory = new LoggerFactory();
			logger = factory.CreateLogger("TEST2");
			factory.AddProvider(new FileLoggerProvider(tmpFile, true));
			logger.LogInformation("Just message");
			factory.Dispose();

			Assert.Equal(3, File.ReadAllLines(tmpFile).Length);

		}
		finally
		{
			File.Delete(tmpFile);
		}
	}

	[Fact]
	public void WriteAscendingRollingFile()
	{
		var tmpFileDir = Path.GetTempFileName(); // for test debug: "./"
		File.Delete(tmpFileDir);

		Directory.CreateDirectory(tmpFileDir);
		try
		{
			var logFile = Path.Combine(tmpFileDir, "test.log");

			LoggerFactory? factory;
			ILogger? logger;
			CreateFactoryAndTestLogger();

			for (int i = 0; i < 400; i++)
			{
				logger?.LogInformation("TEST 0123456789");

				if (i % 50 == 0)
				{
					Thread.Sleep(50); // give some time for a log writer to handle the queue
				}
			}

			factory?.Dispose();

			// check how many files are created
			Assert.Equal(4, Directory.GetFiles(tmpFileDir, "test*.log").Length);

			var lastFileSize = new FileInfo(Path.Combine(tmpFileDir, "test3.log")).Length;

			// create a new factory and continue
			CreateFactoryAndTestLogger();
			logger?.LogInformation("TEST 0123456789");
			factory?.Dispose();

			Assert.True(new FileInfo(Path.Combine(tmpFileDir, "test3.log")).Length > lastFileSize);

			// add many entries and ensure that there are only 5 log files
			CreateFactoryAndTestLogger();

			for (int i = 0; i < 1000; i++)
			{
				logger?.LogInformation("TEST 0123456789");
			}

			factory?.Dispose();

			Assert.Equal(5, Directory.GetFiles(tmpFileDir, "test*.log").Length);

			void CreateFactoryAndTestLogger()
			{
				factory = new LoggerFactory();

				factory.AddProvider(new FileLoggerProvider(logFile, new FileLoggerOptions()
				{
					FileSizeLimitBytes = 1024 * 8,
					MaxRollingFiles = 5
				}));

				logger = factory.CreateLogger("TEST");
			}

		}
		finally
		{
			Directory.Delete(tmpFileDir, true);
		}
	}

	[Fact]
	public void WriteAscendingStableBaseRollingFile()
	{
		var tmpFileDir = Path.GetTempFileName(); // for test debug: "./"
		File.Delete(tmpFileDir);

		Directory.CreateDirectory(tmpFileDir);

		try
		{
			var logFile = Path.Combine(tmpFileDir, "test.log");

			LoggerFactory? factory;
			ILogger? logger;
			CreateFactoryAndTestLogger();

			for (int i = 0; i < 400; i++)
			{
				logger?.LogInformation("TEST 0123456789");

				if (i % 50 == 0)
				{
					Thread.Sleep(50); // give some time for log writer to handle the queue
				}
			}

			factory?.Dispose();

			// To check how many files are created, Stablebase will always write to test.log
			Assert.Equal(4, Directory.GetFiles(tmpFileDir, "test*.log").Length);

			var lastFileSize = new FileInfo(Path.Combine(tmpFileDir, "test.log")).Length;

			// create a new factory and continue
			CreateFactoryAndTestLogger();
			logger?.LogInformation("TEST 0123456789");
			factory?.Dispose();

			Assert.True(new FileInfo(Path.Combine(tmpFileDir, "test.log")).Length > lastFileSize);

			// add many entries and ensure that there are only 5 log files
			CreateFactoryAndTestLogger();

			for (int i = 0; i < 1000; i++)
			{
				logger?.LogInformation("TEST 0123456789");
			}

			factory?.Dispose();

			Assert.Equal(5, Directory.GetFiles(tmpFileDir, "test*.log").Length);

			void CreateFactoryAndTestLogger()
			{
				factory = new LoggerFactory();

				factory.AddProvider(new FileLoggerProvider(logFile, new FileLoggerOptions()
				{
					FileSizeLimitBytes = 1024 * 8,
					MaxRollingFiles = 5,
					RollingFilesConvention = FileLoggerOptions.FileRollingConvention.AscendingStableBase
				}));

				logger = factory.CreateLogger("TEST");
			}

		}
		finally
		{
			Directory.Delete(tmpFileDir, true);
		}
	}

	private static readonly string[] ExpectedDescendingRollingFiles = 
		["test4.log", "test3.log", "test2.log", "test1.log", "test.log"];

	[Fact]
	public void WriteDescendingRollingFile()
	{
		var tmpFileDir = Path.GetTempFileName(); // for test debug: "./"
		File.Delete(tmpFileDir);

		Directory.CreateDirectory(tmpFileDir);

		try
		{
			var logFile = Path.Combine(tmpFileDir, "test.log");

			LoggerFactory? factory;
			ILogger? logger;
			CreateFactoryAndTestLogger();

			for (int i = 0; i < 400; i++)
			{
				logger?.LogInformation("TEST 0123456789");

				if (i % 50 == 0)
				{
					Thread.Sleep(50); // give some time for log writer to handle the queue
				}
			}

			factory?.Dispose();

			// check how many files are created, Descending will always write to test.log
			Assert.Equal(4, Directory.GetFiles(tmpFileDir, "test*.log").Length);

			var lastFileSize = new FileInfo(Path.Combine(tmpFileDir, "test.log")).Length;

			// create a new factory and continue
			CreateFactoryAndTestLogger();
			logger?.LogInformation("TEST 0123456789");
			factory?.Dispose();

			Assert.True(new FileInfo(Path.Combine(tmpFileDir, "test.log")).Length > lastFileSize);

			// add many entries and ensure that there are only 5 log files
			CreateFactoryAndTestLogger();

			for (int i = 0; i < 1000; i++)
			{
				logger?.LogInformation("TEST 0123456789");

				if (i % 50 == 0)
				{
					Thread.Sleep(50); // give some time for a log writer to handle the queue
				}
			}

			factory?.Dispose();

			Assert.Equal(5, Directory.GetFiles(tmpFileDir, "test*.log").Length);

			//check last time written in order
			Assert.Equal(ExpectedDescendingRollingFiles, new DirectoryInfo(tmpFileDir)
				.GetFiles("test*.log")
				.OrderBy(f => f.LastWriteTimeUtc)
				.Select(f => f.Name)
				.ToArray());

			void CreateFactoryAndTestLogger()
			{
				factory = new LoggerFactory();
				
				factory.AddProvider(new FileLoggerProvider(logFile, new FileLoggerOptions()
				{
					FileSizeLimitBytes = 1024 * 8,
					MaxRollingFiles = 5,
					RollingFilesConvention = FileLoggerOptions.FileRollingConvention.Descending
				}));
				
				logger = factory.CreateLogger("TEST");
			}
		}
		finally
		{
			Directory.Delete(tmpFileDir, true);
		}
	}

	[Fact]
	public async Task WriteConcurrent()
	{
		var tmpFile = Path.GetTempFileName();
		
		try
		{
			var factory = new LoggerFactory();
			factory.AddProvider(new FileLoggerProvider(tmpFile));
			var writeTasks = new List<Task>();
			
			for (int i = 0; i < 5; i++)
			{
				writeTasks.Add(
					Task.Factory.StartNew(() =>
					{
						var logger = factory.CreateLogger("TEST" + i);
						
						for (int j = 0; j < 100000; j++)
						{
							logger.LogInformation("MSG" + j);
						}
					})
				);
			}

			await Task.WhenAll(writeTasks.ToArray());

			factory.Dispose();
			var lines = 0;

			await using (var fileStream = new FileStream(tmpFile, FileMode.Open, FileAccess.Read))
			{
				using (var rdr = new StreamReader(fileStream))
				{
					while (true)
					{
						var s = await rdr.ReadLineAsync();
						
						if (s != null)
						{
							lines++;
						}
						else
						{
							break;
						}
					}
				}
			}

			Assert.Equal(500000, lines);
		}
		finally
		{
			File.Delete(tmpFile);
		}
	}

	[Fact]
	public void CreateDirectoryAutomatically()
	{
		var tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "testfile.log");
		
		try
		{
			var factory = new LoggerFactory();

			factory.AddProvider(new FileLoggerProvider(tmpFile, new FileLoggerOptions()));

			var logger = factory.CreateLogger("TEST");
			logger.LogInformation("Line1");
			factory.Dispose();

			Assert.Single(File.ReadAllLines(tmpFile));
		}
		finally
		{
			var directory = Path.GetDirectoryName(tmpFile);
			
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
			}
		}
	}

	[Fact]
	public void ExpandEnvironmentVariables()
	{
		// this is a windows-only test
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return;

		var tmpFileWithEnvironmentVariable = "%TEMP%\\" + Path.GetFileName(Path.GetTempFileName());
		var expandedTmpFileName = Path.Combine(Path.GetTempPath(), Path.GetFileName(tmpFileWithEnvironmentVariable));
		
		try
		{
			var factory = new LoggerFactory();
			factory.AddProvider(new FileLoggerProvider(tmpFileWithEnvironmentVariable, false));
			var logger = factory.CreateLogger("TEST");
			
			logger.LogInformation("Line1");
			factory.Dispose();

			Assert.Single(File.ReadAllLines(expandedTmpFileName));
		}
		finally
		{
			File.Delete(expandedTmpFileName);
		}
	}

	[Fact]
	public void CustomLogFileNameFormatter()
	{
		var tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "testfile_{0}.log");

		try
		{
			var factory = new LoggerFactory();
			var fmtLogFileNameIdx = 0;

			factory.AddProvider(new FileLoggerProvider(tmpFile, new FileLoggerOptions
			{
				FormatLogFileName = fName => string.Format(fName, fmtLogFileNameIdx)
			}));

			var logger = factory.CreateLogger("TEST");
			
			for (int i = 0; i < 15; i++)
			{
				fmtLogFileNameIdx = i / 5;

				if (i > 0 && i % 5 == 0)
				{
					Thread.Sleep(1000); // log writer works in another thread. Let him process log messages in the queue.
				}

				logger.LogInformation("Line" + (i + 1));
			}

			factory.Dispose();

			// should be 3 files, each with 5 lines
			var logFiles = Directory.GetFiles(
				Path.GetDirectoryName(tmpFile) ?? string.Empty, 
				"*.*", 
				SearchOption.TopDirectoryOnly);
			
			Assert.Equal(3, logFiles.Length);
		}
		finally
		{
			var directory = Path.GetDirectoryName(tmpFile);
			
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
			}
		}

	}

	void CleanupTempDir(string tmpDir, IEnumerable<IDisposable> toDispose)
	{
		try
		{
			foreach (var o in toDispose)
			{
				o.Dispose();
			}

			if (Directory.Exists(tmpDir))
			{
				Directory.Delete(tmpDir, true);
			}
		}
		catch
		{
			// ignore cleanup errs
		}
	}

	[Fact]
	public void FileOpenErrorHandling()
	{
		// this is a windows-only test
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return;
		}

		var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		var toDispose = new List<IDisposable>();
		
		try
		{
			var logFileName = Path.Combine(tmpDir, "testfile.log");

			var factory = new LoggerFactory();
			toDispose.Add(factory);
			factory.AddProvider(new FileLoggerProvider(logFileName, new FileLoggerOptions()));
			var logger = factory.CreateLogger("TEST");
			WriteSomethingToLogger(logger, 15);

			Assert.Throws<IOException>(() =>
			{
				// now try to use a currently used file in another logger
				var factory2 = new LoggerFactory();
				factory2.AddProvider(new FileLoggerProvider(logFileName, new FileLoggerOptions()));
				var logger2 = factory2.CreateLogger("TEST");
				WriteSomethingToLogger(logger2, 15);
			});

			var errorHandled = false;
			var factory3 = new LoggerFactory();
			toDispose.Add(factory3);
			var altLogFileName = Path.Combine(tmpDir, "testfile_after_err.log");
			
			factory3.AddProvider(new FileLoggerProvider(logFileName, new FileLoggerOptions()
			{
				HandleFileError = (err) =>
				{
					errorHandled = true;
					err.UseNewLogFileName(altLogFileName);
				}
			}));
			
			var logger3 = factory3.CreateLogger("TEST");
			WriteSomethingToLogger(logger3, 15);
			
			Assert.True(errorHandled);
			
			factory3.Dispose();
			
			// ensure that alt file name was used
			var altLogFileInfo = new FileInfo(altLogFileName);
			
			Assert.True(altLogFileInfo.Exists);
			Assert.True(altLogFileInfo.Length > 0);
		}
		finally
		{
			CleanupTempDir(tmpDir, toDispose);
		}

	}

	private static void WriteSomethingToLogger(ILogger log, int msgCount)
	{
		for (int i = 0; i < msgCount; i++)
		{
			log.LogInformation("Line" + (i + 1));
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FileWriteErrorHandling(bool useNewLogFile)
	{
		var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		var logFileName = Path.Combine(tmpDir, "testfile.log");
		var fallbackLogFileName = Path.Combine(tmpDir, "fallbackfile.log");

		var factory = new LoggerFactory();
		var fileLoggerOpts = new FileLoggerOptions();
		
		if (useNewLogFile)
		{
			fileLoggerOpts.HandleFileError = fileErr => fileErr.UseNewLogFileName(fallbackLogFileName);
		}

		var fileLogPrv = new FileLoggerProvider(logFileName, fileLoggerOpts);
		factory.AddProvider(fileLogPrv);
		var logger = factory.CreateLogger("TEST");
		WriteSomethingToLogger(logger, 1);


		var logFileWr = fileLogPrv.GetType()
			.GetField("_fWriter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
			.GetValue(fileLogPrv);
		
		// close file handler, this will cause an exception inside FileLoggerProvider.ProcessQueue
		var logFileStream = (Stream)logFileWr!.GetType()
			.GetField("_LogFileStream",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
			.GetValue(logFileWr)!;
		
		logFileStream.Close();

		var t = Task.Run(() => { WriteSomethingToLogger(logger, 2000); });
		
#pragma warning disable xUnit1031
		Assert.True(t.Wait(1000), "Logger queue blocks app's log calls.");
#pragma warning restore xUnit1031

		factory.Dispose();

		if (useNewLogFile)
		{
			// ensure that it is not empty and was used
			Assert.True(new FileInfo(fallbackLogFileName).Length > 0,
				"Alternative log file name was not used.");
		}
	}

	[Fact]
	public void CustomFilterLogEntry()
	{
		var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		var logFileName = Path.Combine(tmpDir, "testfile.log");
		var factory = new LoggerFactory();
		
		try
		{
			factory.AddProvider(new FileLoggerProvider(logFileName, new FileLoggerOptions()
			{
				FilterLogEntry = logEntry => logEntry.LogName == "TEST"
			}));
			
			var testLogger = factory.CreateLogger("TEST");
			var test2Logger = factory.CreateLogger("TEST2");

			testLogger.LogInformation("TEST");
			test2Logger.LogInformation("TEST2");

			factory.Dispose();

			var logLines = File.ReadAllLines(logFileName);
			
			Assert.Single(logLines);
			Assert.DoesNotContain("TEST2", logLines[0]);
		}
		finally
		{
			CleanupTempDir(tmpDir, [factory]);
		}
	}
}