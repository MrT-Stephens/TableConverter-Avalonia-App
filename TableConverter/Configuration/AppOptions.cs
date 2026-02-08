using System;
using System.IO;
using SukiUI.Enums;

namespace TableConverter.Configuration;

public class AppOptions
{
    /// <summary>
    /// The name of the application, used for configuration and logging purposes.
    /// </summary>
    public string AppName { get; set; } = "TableConverter";

    /// <summary>
    /// The background style for the application, which can be set to a predefined style from the SukiBackgroundStyle enum.
    /// </summary>
    public SukiBackgroundStyle BackgroundStyle { get; set; } = SukiBackgroundStyle.GradientDarker;
    
    /// <summary>
    /// Whether the background animation is enabled.
    /// </summary>
    public bool BackgroundAnimationEnabled { get; set; } = false;
    
    /// <summary>
    /// Whether the background transition is enabled.
    /// </summary>
    public bool BackgroundTransitionEnabled { get; set; } = false;

    /// <summary>
    /// The theme color of the application.
    /// </summary>
    public SukiColor ThemeColor { get; set; } = SukiColor.Green;
    
    /// <summary>
    /// The working directory for the application.
    /// </summary>
    public Environment.SpecialFolder WorkingDirectory { get; set; } = Environment.SpecialFolder.ApplicationData;

    /// <summary>
    /// The name of the directory to use for storing documents.
    /// </summary>
    public string DocumentsDirectory { get; set; } = "Documents";
    
    /// <summary>
    /// The name of the directory to use for storing configuration files.
    /// </summary>
    public string ConfigDirectory { get; set; } = "Config";

    /// <summary>
    /// The base content path for the application.
    /// </summary>
    public string BaseContentPath => Path.Combine(Environment.GetFolderPath(WorkingDirectory), AppName);
    
    /// <summary>
    /// The base path for configuration files.
    /// </summary>
    public string BaseConfigPath => Path.Combine(BaseContentPath, ConfigDirectory);
    
    /// <summary>
    /// The base path for documents.
    /// </summary>
    public string BaseDocumentsPath => Path.Combine(BaseContentPath, DocumentsDirectory);
}