using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections;
using ForestM8rix;

namespace ForestM8rix.Benchmarks;

// [СУТЬ] Тест скорости индексации 1 000 000 узлов
[MemoryDiagnoser]
public class RegistryBenchmark
{
    public class MyNode // Локальная модель для чистоты теста
    {
        public uint ForestM8rixId { get; set; }
        public List<MyNode> Nodes { get; set; } = new();
    }

    private List<MyNode> _data;
    private readonly NodeRegistry _registry = new();
    private readonly Func<object, IEnumerable> _resolver = n => ((MyNode)n).Nodes;

    private uint _nextId;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, 1000).Select(i => new MyNode
        {
            Nodes = Enumerable.Range(0, 1000).Select(j => new MyNode()).ToList()
        }).ToList();
    }

    [Benchmark]
    // [СУТЬ] Теперь метод называется Process (объединяет ID и Flattening)
    public void RegisterMillion() => _registry.Process(_data, _resolver);

    //public static void Main() => BenchmarkRunner.Run<RegistryBenchmark>();


    [Benchmark]
    public void RegisterWithReflection() // Прямая рефлексия (Медленно)
    {
        _nextId = 0;
        foreach (var node in _data) RegisterRecursiveReflect(node);
    }

    private void RegisterRecursiveReflect(MyNode node)
    {
        // Каждый раз ищем свойство заново
        var prop = node.GetType().GetProperty("ForestM8rixId");
        prop?.SetValue(node, _nextId++);

        if (node.Nodes != null)
            foreach (var child in node.Nodes) RegisterRecursiveReflect(child);
    }
}