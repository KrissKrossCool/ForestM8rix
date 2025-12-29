using ForestM8rix;
using ForestM8rix.StateManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ForestM8rix.WPF.Demo;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //this.Loaded += OnLoaded;

        // Создаем список ЯВНО
        var data = new List<Item>();
        data.Add(new Item { Title = "Чапаев" });
        data.Add(new Item { Title = "Петька" });

        // [СУТЬ] Сначала раскрываем программно через ВАШ реестр
        ForestStateRegistry.SetExpanded(data[0], true);

        ForestView.SetData(data, x => ((Item)x).Children);

        // ПЕРЕДАЕМ (Проверьте, что имя ForestView совпадает с x:Name в XAML)
        //this.ForestView.SetData(list, x => ((Item)x).Children);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 1. Создаем "мясо" для Чапаева
        var data = new List<Item>
    {
        new Item
        {
            Title = "Чапаев (Корень)",
            Children = new List<Item> { new Item { Title = "Петька (Сын)" } }
        }
    };

        // [СУТЬ] 2. ПЕРЕДАЧА. Проверьте, что селектор возвращает Children
        ForestView.SetData(data, x => ((Item)x).Children);
    }
}

public class NodeItem
{
    public string Title { get; set; } = "";
    public uint ForestM8rixId { get; set; } // Для нашего движка
    public List<NodeItem>? Children { get; set; }

    // [СУТЬ] Чтобы текст отображался в OnRender
    public override string ToString() => Title;
}

public class Item
{
    public string Title { get; set; }
    //public uint ForestM8rixId { get; set; }
    public List<Item> Children { get; set; } = new();
    public override string ToString() => Title;
}
