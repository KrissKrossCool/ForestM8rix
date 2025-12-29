using BenchmarkDotNet.Attributes;
using System.Collections;

namespace ForestM8rix.Benchmarks;

[MemoryDiagnoser]
public class ForestM8rixFullPerformanceBenchmark
{
    private NodeRegistry _registry = null!;
    private List<TestData> _data = null!;
    private Func<object, IEnumerable> _resolver = null!;

    private const double RowHeight = 25.0;
    private const double ViewportHeight = 1000.0;
    private const double ScrollOffset = 500000.0; // Скролл в середину миллиона

    [GlobalSetup]
    public void Setup()
    {
        _registry = new NodeRegistry();
        _data = new List<TestData>(1_000_000);
        for (int i = 0; i < 1_000_000; i++)
            _data.Add(new TestData());

        _resolver = obj => ((TestData)obj).Children;
    }

    // 1. Тест скорости индексации и построения плоского списка
    [Benchmark]
    public void FullProcessMillion()
    {
        _registry.Process(_data, _resolver);
    }

    // 2. Тест скорости расчета кадра (сколько наносекунд на 1 кадр)
    [Benchmark]
    public (int start, int end) CalculateViewportFrame()
    {
        int start = (int)(ScrollOffset / RowHeight);
        int end = (int)((ScrollOffset + ViewportHeight) / RowHeight) + 1;

        if (start < 0) start = 0;
        if (end >= _registry.Count) end = _registry.Count - 1;

        return (start, end);
    }

    [GlobalCleanup]
    public void Cleanup() => _registry.Dispose();

    public class TestData
    {
        public uint ForestM8rixId { get; set; }
        public List<TestData>? Children { get; set; }
    }
}