using BenchmarkDotNet.Attributes;

namespace ForestM8rix.Benchmarks
{
    // [TAG] Класс с тестами производительности
    [MemoryDiagnoser] // Обязательно: показывает расход RAM
    public class ForestPerformanceTest
    {
        private List<int> _data;

        [Params(1000, 10000)] // Тестируем на разных объемах
        public int Iterations;

        [GlobalSetup]
        public void Setup()
        {
            // Подготовка данных вне замера времени
            _data = new List<int>();
            for (int i = 0; i < Iterations; i++) _data.Add(i);
        }

        [Benchmark]
        public int SimpleSumTest()
        {
            // Базовый тест, чтобы убедиться, что окружение работает
            int sum = 0;
            foreach (var item in _data) sum += item;
            return sum;
        }
    }
}