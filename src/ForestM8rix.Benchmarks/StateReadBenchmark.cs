using BenchmarkDotNet.Attributes;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Benchmarks;

[MemoryDiagnoser] // Покажет потребление памяти в таблице
public class StateReadBenchmark
{
    // Параметры для автоматического создания итоговой таблицы
    [Params(100, 10000, 1000000)]
    public int Count;

    private object[] _nodes;
    private Dictionary<object, bool> _dictionary;

    [GlobalSetup]
    public void Setup()
    {
        _nodes = new object[Count];
        _dictionary = new Dictionary<object, bool>(Count);

        for (int i = 0; i < Count; i++)
        {
            _nodes[i] = new object();

            // Наполняем данными
            ForestStateRegistry.SetSelected(_nodes[i], i % 2 == 0);
            _dictionary[_nodes[i]] = i % 2 == 0;
        }
    }

    [Benchmark(Baseline = true)] // Эталон для сравнения
    public bool Dictionary_Read()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _dictionary[_nodes[i]];
        return result;
    }

    [Benchmark]
    public bool Registry_Read()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = ForestStateRegistry.IsSelected(_nodes[i]);
        return result;
    }
}