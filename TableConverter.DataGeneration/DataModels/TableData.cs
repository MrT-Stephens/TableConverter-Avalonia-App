namespace TableConverter.DataGeneration.DataModels;

public record TableData(
    List<string> Headers,
    List<string[]> Rows
);