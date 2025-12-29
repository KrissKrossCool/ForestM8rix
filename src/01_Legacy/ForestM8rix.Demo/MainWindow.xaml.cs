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
        this.Loaded += OnLoaded;
    }

    // 2. Настройка в MainWindow
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var mgr = ForestDisplay.Manager;

        mgr.Columns.Clear();

        // Красивые рабочие колонки
        mgr.Columns.Add(new ForestColumn("Название", 250, n => (n as DemoItem)?.Title));
        mgr.Columns.Add(new ForestColumn("Тип", 80, n => (n as DemoItem)?.Extension));
        mgr.Columns.Add(new ForestColumn("Размер (МБ)", 100, n => (n as DemoItem)?.Size.ToString("F2")));

        var data = GenerateDemoFiles();
        mgr.SetSource(data, n => (n as DemoItem)?.SubItems);
    }

    private List<DemoItem> GenerateDemoFiles()
    {
        return new List<DemoItem>
        {
            new DemoItem("Проект_Альфа", "Папка", 0) {
                SubItems = new List<DemoItem> {
                    new DemoItem("Main.cs", "Файл", 1.2),
                    new DemoItem("Styles.xaml", "Файл", 0.5),
                    new DemoItem("Assets", "Папка", 0) {
                        SubItems = new List<DemoItem> {
                            new DemoItem("Logo.png", "Изображение", 2.4)
                        }
                    }
                }
            },
            new DemoItem("Архив_2025", "Папка", 450.0)
        };
    }
}

// 1. Класс данных (Свойства должны быть публичными)
public class DemoItem
{
    public string Title { get; set; }
    public string Extension { get; set; }
    public double Size { get; set; }
    public List<DemoItem> SubItems { get; set; }

    public DemoItem(string title, string ext, double size)
    {
        Title = title;
        Extension = ext;
        Size = size;
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
