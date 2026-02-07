using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Logging.Config;

namespace TableConverter.Utilities.Logging;

public static class FileLoggerExtensions
{
	/// <summary>
	/// Adds a file logger.
	/// </summary>
	public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string fileName, bool append = true)
	{
		builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(
			_ => new FileLoggerProvider(fileName, append)));
		
		return builder;
	}

	/// <summary>
	/// Adds a file logger.
	/// </summary>
	public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string fileName, Action<FileLoggerOptions> configure)
	{
		builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(_ =>
			{
				var options = new FileLoggerOptions();
				configure(options);
				return new FileLoggerProvider(fileName, options);
			}
		));
		
		return builder;
	}

	/// <summary>
	/// Adds a file logger by specified configuration.
	/// </summary>
	/// <remarks>File logger is not added if the "File" section is not present, or it doesn't contain the "Path" property.</remarks>
	public static ILoggingBuilder AddFile(this ILoggingBuilder builder, IConfiguration configuration, Action<FileLoggerOptions>? configure = null)
	{
		var fileLoggerOptions = GetOptionsFromConfiguration(configuration, configure);
		
		if (fileLoggerOptions != null)
		{
			builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(
				_ => new FileLoggerProvider(fileLoggerOptions.Item1, fileLoggerOptions.Item2));
		}

		return builder;
	}

	/// <summary>
	/// Adds a file logger.
	/// </summary>
	/// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
	/// <param name="fileName">log file name.</param>
	/// <param name="append">if true, new log entries are appended to the existing file.</param>	 
	public static ILoggerFactory AddFile(this ILoggerFactory factory, string fileName, bool append = true)
	{
		factory.AddProvider(new FileLoggerProvider(fileName, append));
		
		return factory;
	}

	/// <summary>
	/// Adds a file logger.
	/// </summary>
	/// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
	/// <param name="fileName">log file name.</param>
	/// <param name="configure">a handler that initializes <see cref="FileLoggerOptions"/>.</param>
	public static ILoggerFactory AddFile(this ILoggerFactory factory, string fileName, Action<FileLoggerOptions> configure)
	{
		var fileLoggerOptions = new FileLoggerOptions();
		configure(fileLoggerOptions);
		factory.AddProvider(new FileLoggerProvider(fileName, fileLoggerOptions));
		return factory;
	}

	/// <summary>
	/// Adds a file logger and configures it with given <see cref="IConfiguration"/> (usually "Logging" section).
	/// </summary>
	/// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
	/// <param name="configuration">The <see cref="IConfiguration"/> to use getting <see cref="FileLoggerProvider"/> settings.</param>
	/// <param name="configure">a handler that initializes <see cref="FileLoggerOptions"/>.</param>
	public static ILoggerFactory AddFile(this ILoggerFactory factory, IConfiguration configuration, Action<FileLoggerOptions>? configure = null)
	{
		var prvFactory = factory;
		var fileLoggerOptions = GetOptionsFromConfiguration(configuration, configure);

		if (fileLoggerOptions == null)
		{
			return factory;
		}

		prvFactory.AddProvider(new FileLoggerProvider(fileLoggerOptions.Item1, fileLoggerOptions.Item2));
		
		return factory;
	}

	private static Tuple<string, FileLoggerOptions>? GetOptionsFromConfiguration(IConfiguration configuration, Action<FileLoggerOptions>? configure)
	{
		var config = new FileLoggerConfig();
		var fileSection = configuration.GetSection("File");
		
		if (!fileSection.Exists())
		{
			var pathValue = configuration["Path"];

			if (string.IsNullOrEmpty(pathValue))
			{
				return null; // the file logger is not configured
			}
			else
			{
				// configuration contains a "Path" property, so this is explicitly passed configuration section
				configuration.Bind(config);
			}
		}
		else
		{
			fileSection.Bind(config);
		}

		if (string.IsNullOrWhiteSpace(config.Path))
		{
			return null; // The file logger is not configured
		}

		var fileLoggerOptions = new FileLoggerOptions
		{
			Append = config.Append,
			MinLevel = config.MinLevel,
			FileSizeLimitBytes = config.FileSizeLimitBytes,
			MaxRollingFiles = config.MaxRollingFiles,
		};

		if (configure != null)
		{
			configure(fileLoggerOptions);
		}

		return new Tuple<string, FileLoggerOptions>(config.Path, fileLoggerOptions);
	}
}