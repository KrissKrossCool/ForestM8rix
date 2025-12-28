using BenchmarkDotNet.Running;
using System.Collections.Generic;

namespace ForestM8rix.Benchmarks;

// [TAG] Точка входа в приложение
public class Program
{
    public static void Main(string[] args)
    {
        // [TAG] Старый тест (сохраняем для истории)
        // BenchmarkRunner.Run<ForestPerformanceTest>();

        // [TAG] Тест на 1 000 000 узлов через ForestFilterService
        //BenchmarkRunner.Run<ForestFilterBenchmarks>();
    }
}