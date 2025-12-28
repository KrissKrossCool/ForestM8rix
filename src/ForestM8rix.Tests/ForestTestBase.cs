using System;
using System.Collections.Generic;
using System.Text;

namespace ForestM8rix.Tests;

public abstract class ForestTestBase
{
    protected const int OneMillion = 1_000_000;
    
    // Метод для быстрого замера времени выполнения в тестах
    protected void Measure(string label, Action action)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        action();
        Console.WriteLine($"{label}: {sw.ElapsedMilliseconds}ms");
    }
}