using BenchmarkDotNet.Attributes;
using ForestM8rix.Core.Interfaces;
using ForestM8rix.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace ForestM8rix.Benchmarks
{
    [MemoryDiagnoser] // [TAG] Анализ потребления оперативной памяти
    public class FilterBenchmark
    {
        private List<IForestM8rixNode> _data = null!;
        private readonly ForestFilterService _service = new();

        [GlobalSetup]
        public void Setup()
        {
            // Генерация иерархии: 1000 корней по 1000 детей = 1 000 000 узлов
            _data = Enumerable.Range(0, 1000).Select(i => new NodeModel
            {
                DisplayName = $"Root {i}",
                Children = Enumerable.Range(0, 1000).Select(j => new NodeModel
                {
                    DisplayName = $"Child {i}-{j}"
                }).Cast<IForestM8rixNode>().ToList()
            }).Cast<IForestM8rixNode>().ToList();
        }

        [Benchmark]
        public void FilterMillionNodes()
        {
            // Имитация поиска пользователя
            _service.ApplyFilter(_data, "999");
        }
    }
}