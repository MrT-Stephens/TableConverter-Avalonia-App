using System.Collections.Generic;

namespace TableConverter.DataModels
{
    public record TableData(
        List<string> headers,
        List<string[]> rows
    );
}
