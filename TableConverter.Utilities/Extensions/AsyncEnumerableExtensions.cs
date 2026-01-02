namespace TableConverter.Utilities.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<List<T>> Batch<T>(this IAsyncEnumerable<T> source, int batchSize)
    {
        var batch = new List<T>(batchSize);
        
        await foreach (var item in source)
        {
            batch.Add(item);
            
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}
