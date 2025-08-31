using System;
using TableConverter.DataModels.Dtos;

namespace TableConverter.DataModels.Events;

public enum  FileChangedEventAction
{
    Added,
    Removed
}

public class FileChangedEventEventArgs(FileInfoDto fileInfo, FileChangedEventAction changeType) : EventArgs
{
    public FileInfoDto FileInfo { get; } = fileInfo;
    public FileChangedEventAction ChangeType { get; } = changeType;
}

public class FileChangedEvent : EventHandlerBase<FileChangedEventEventArgs>
{
}