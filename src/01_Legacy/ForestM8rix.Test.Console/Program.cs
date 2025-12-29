using ForestM8rix.StateManagement;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ForestM8rix.Test.AppConsole;



// 1. Наш интерфейс в стиле C# 10
public interface IForestNotify : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;
    // Реализация по умолчанию (DIM)
    public void NotifyDIM([CallerMemberName] string prop = "")
    {
        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        // ВНИМАНИЕ: Мы не можем здесь вызвать PropertyChanged, 
        // так как интерфейс им не "владеет". 
        // Компилятор просто не даст написать PropertyChanged?.Invoke(...)
        Console.WriteLine($"   [IDIM] Вызван метод NotifyDIM для: {prop} (Но событие не поднять!)");
    }
}

// 2. Имитация старой библиотеки (SharpTreeNode)
public class OldBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

// 3. Наш экспериментальный класс
public class TestNode : OldBase, IForestNotify
{
    private string _name = "";

    // ВЕРНЫЙ ВАРИАНТ: Проброс через класс
    public void NotifyCorrect([CallerMemberName] string prop = "")
        => RaisePropertyChanged(prop);

    public string Name
    {
        get => _name;
        set => _name = value;
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        ForestBenchmark benchmark = new ForestBenchmark();
        benchmark.Run();

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }
}

public class ForestBenchmark
{
    class DuckNode { public uint ForestM8rixId { get; set; } }
    class CleanNode { }

    public void Run()
    {
        int count = 10_000_000;
        var ducks = new List<DuckNode>(count);
        var cleans = new List<CleanNode>(count);

        for (int i = 0; i < count; i++)
        {
            ducks.Add(new DuckNode());
            cleans.Add(new CleanNode());
        }

        Console.WriteLine($"--- Тест на {count} объектов ---");

        // 1. Замер: Duck Typing (Уточка)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
            ForestStateRegistry.SetExpanded(ducks[i], true);
        sw.Stop();
        Console.WriteLine($"Duck Path (ID Field): {sw.ElapsedMilliseconds} ms");

        // 2. Замер: ConditionalWeakTable (Красава)
        sw.Restart();
        for (int i = 0; i < count; i++)
            ForestStateRegistry.SetExpanded(cleans[i], true);
        sw.Stop();
        Console.WriteLine($"Standard Path (CWT): {sw.ElapsedMilliseconds} ms");

        // 3. Валидация
        bool allOk = ForestStateRegistry.IsExpanded(ducks[500]) &&
                     ForestStateRegistry.IsExpanded(cleans[500]);
        Console.WriteLine($"[VALIDATION] Данные сохранены: {allOk}");
    }
}
