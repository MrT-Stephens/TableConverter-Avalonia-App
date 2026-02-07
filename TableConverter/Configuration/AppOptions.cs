using System;
using System.IO;

namespace TableConverter.Configuration;

public class AppOptions
{
    /// <summary>
    /// The name of the application, used for configuration and logging purposes.
    /// </summary>
    public string AppName { get; set; } = "TableConverter";
    
    /// <summary>
    /// The working directory for the application.
    /// </summary>
    public Environment.SpecialFolder WorkingDirectory { get; set; } = Environment.SpecialFolder.ApplicationData;

    /// <summary>
    /// The base content path for the application.
    /// </summary>
    public string BaseContentPath => Path.Combine(Environment.GetFolderPath(WorkingDirectory), AppName);
}