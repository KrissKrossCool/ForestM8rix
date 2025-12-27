using BenchmarkDotNet.Attributes;
using ForestM8rix.Core.Interfaces;
using ForestM8rix.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace ForestM8rix.Benchmarks
{
    [MemoryDiagnoser]
    public class ForestFilterBenchmarks
    {
        private ForestFilterService _service = new();
        private List<IForestM8rixNode> _tree = new();

        [GlobalSetup]
        public void Setup()
        {
            // Генерируем 1 000 000 узлов (1000 корней по 1000 детей)
            _tree = Enumerable.Range(0, 1000).Select(i => new Node
            {
                DisplayName = $"Root {i}",
                Children = Enumerable.Range(0, 1000).Select(j => new Node
                {
                    DisplayName = $"Child {i}-{j}"
                }).Cast<IForestM8rixNode>().ToList()
            }).Cast<IForestM8rixNode>().ToList();
        }

        [Benchmark]
        public void TestFilterMillion()
        {
            _service.ApplyFilter(_tree, "999");
        }
    }
}