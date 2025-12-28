using System;
using System.ComponentModel;
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
        var node = new TestNode();

        // Подписка (как это делает WPF в фоне)
        node.PropertyChanged += (s, e) =>
            Console.WriteLine($"   [UI] !!! Ура! Получено уведомление о: {e.PropertyName}");

        Console.WriteLine("--- УСЛОВИЕ 1: ЛОЖНОЕ (Только DIM) ---");
        node.Name = "Ошибка";
        ((IForestNotify)node).NotifyDIM("Name");
        Console.WriteLine("Результат: UI промолчал.");

        Console.WriteLine("\n--- УСЛОВИЕ 2: ВЕРНОЕ (Проброс в базу) ---");
        node.Name = "Успех";
        node.NotifyCorrect("Name");
        Console.WriteLine("Результат: UI обновился.");

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }
}
