using System;
using System.IO;

namespace TableConverter.DataModels;

public record FileDialogManagerFile(
    string Name,
    Uri Path,
    Stream Stream
);