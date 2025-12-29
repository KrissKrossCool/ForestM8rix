using BenchmarkDotNet.Running;

namespace ForestM8rix.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        // Явно указываем, какой класс запускать. 
        // Если здесь будет ошибка компиляции — вы узнаете о ней сразу.
        BenchmarkRunner.Run<ForestM8rixFullPerformanceBenchmark>();
    }
}