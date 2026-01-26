using BenchmarkDotNet.Attributes;

namespace Rebels.Temporal.Benchmarks;

[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class ConsumerBenchmark
{
    private const int Iterations = 10000;

    // =====================================================
    // APPROACH 1: Concrete ref struct (current approach)
    // =====================================================

    [Benchmark(Baseline = true)]
    public int ConcreteRefStruct()
    {
        var buffer = new ConcreteBuffer();
        AddConcrete(ref buffer, Iterations);
        return buffer.Count;
    }

    private static void AddConcrete(ref ConcreteBuffer buffer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buffer.Add(i);
        }
    }

    // =====================================================
    // APPROACH 2: Generic with struct constraint + interface
    // =====================================================

    [Benchmark]
    public int GenericStructConstraint()
    {
        var buffer = new GenericBuffer();
        AddGeneric(ref buffer, Iterations);
        return buffer.Count;
    }

    private static void AddGeneric<T>(ref T buffer, int count)
        where T : struct, IBuffer
    {
        for (int i = 0; i < count; i++)
        {
            buffer.Add(i);
        }
    }
}

// =====================================================
// Types
// =====================================================

public interface IBuffer
{
    void Add(int value);
}

public ref struct ConcreteBuffer
{
    public int Count;
    public void Add(int value) => Count++;
}

public struct GenericBuffer : IBuffer
{
    public int Count;
    public void Add(int value) => Count++;
}
