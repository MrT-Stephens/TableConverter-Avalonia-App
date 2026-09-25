using System.Reflection;

namespace TableConverter.DataGeneration.Tests.FactoryTests;

public class RandomizerTest
{
    [Fact]
    public void Seeded_Randomizers_Produce_The_Same_Sequence()
    {
        var first = new Randomizer(42);
        var second = new Randomizer(42);

        var firstSequence = Enumerable.Range(0, 200).Select(_ => first.Number(0, 1000)).ToArray();
        var secondSequence = Enumerable.Range(0, 200).Select(_ => second.Number(0, 1000)).ToArray();

        Assert.Equal(firstSequence, secondSequence);
    }

    [Fact]
    public void Number_Stays_Within_The_Inclusive_Bounds()
    {
        var random = new Randomizer(7);

        for (var i = 0; i < 1000; i++)
        {
            Assert.InRange(random.Number(5, 10), 5, 10);
        }
    }

    [Fact]
    public void A_Single_Randomizer_Is_Safe_Under_Concurrent_Use()
    {
        var random = new Randomizer(12345);
        const int iterations = 20_000;

        var results = new int[iterations];

        Parallel.For(0, iterations, i => results[i] = random.Number(0, 1000));

        Assert.All(results, value => Assert.InRange(value, 0, 1000));

        // A Random corrupted by unsynchronised concurrent access collapses to a constant (usually 0), so a healthy
        // amount of variety proves the instance is still being guarded.
        Assert.True(results.Distinct().Count() > 100,
            $"Expected a varied sequence but only saw {results.Distinct().Count()} distinct values.");
    }

    [Fact]
    public void Randomizer_Does_Not_Share_A_Process_Wide_Lock()
    {
        // The guarded Random is per-instance, so a static lock would serialise every generator in the process.
        var fields = typeof(Randomizer).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.DoesNotContain(fields, field => !field.IsLiteral && IsSharedObjectHolder(field.FieldType));
    }

    [Fact]
    public void Bytes_Fills_The_Whole_Buffer_Under_Concurrent_Use()
    {
        var random = new Randomizer(3);
        var buffers = new byte[64][];

        Parallel.For(0, buffers.Length, i => buffers[i] = random.Bytes(32));

        Assert.All(buffers, buffer =>
        {
            Assert.Equal(32, buffer.Length);
            Assert.Contains(buffer, b => b != 0);
        });
    }

    [Fact]
    public async Task Concurrent_Instances_Each_Keep_Their_Own_Sequence()
    {
        var expectedA = BuildSequence(new Randomizer(11), 300);
        var expectedB = BuildSequence(new Randomizer(22), 300);

        var subjectA = new Randomizer(11);
        var subjectB = new Randomizer(22);

        var actualA = new int[expectedA.Length];
        var actualB = new int[expectedB.Length];

        // Each array is filled sequentially by exactly one thread, so the index-to-sequence-position mapping holds.
        var taskA = Task.Run(() =>
        {
            for (var i = 0; i < actualA.Length; i++) actualA[i] = subjectA.Number(0, 10_000);
        });

        var taskB = Task.Run(() =>
        {
            for (var i = 0; i < actualB.Length; i++) actualB[i] = subjectB.Number(0, 10_000);
        });

        await Task.WhenAll(taskA, taskB);

        Assert.Equal(expectedA, actualA);
        Assert.Equal(expectedB, actualB);
    }

    private static bool IsSharedObjectHolder(Type type)
    {
        if (type == typeof(object))
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Lazy<>);
    }

    private static int[] BuildSequence(Randomizer randomizer, int length)
    {
        var sequence = new int[length];

        for (var i = 0; i < length; i++) sequence[i] = randomizer.Number(0, 10_000);

        return sequence;
    }
}
