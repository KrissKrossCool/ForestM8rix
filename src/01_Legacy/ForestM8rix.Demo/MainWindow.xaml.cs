using ForestM8rix;
using System.Collections.Generic;
using System.Windows;

namespace ForestM8rix.WPF.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var mgr = ForestDisplay.Manager; // ForestView - имя из XAML

        // 1. НАСТРОЙКА КОЛОНОК
        mgr.Columns.Clear();

        // Используем конструктор, как в вашем примере
        mgr.Columns.Add(new ForestColumn("Название", 250, n => (n as DemoItem)?.Title));
        mgr.Columns.Add(new ForestColumn("ID", 80, n => (n as DemoItem)?.Id.ToString()));
        mgr.Columns.Add(new ForestColumn("Статус", 120, n => (n as DemoItem)?.Status));

        // 2. ГЕНЕРАЦИЯ ДАННЫХ
        var data = GenerateData(1000, 3);

        // 3. ЗАГРУЗКА
        ForestDisplay.SetData(data, node => ((DemoItem)node).Children);
    }

    // Класс данных (переименовал в DemoItem для соответствия сниппету)
    public class DemoItem
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public string Status { get; set; }
        public List<DemoItem> Children { get; set; } = new();
    }

    private List<DemoItem> GenerateData(int count, int depth)
    {
        var list = new List<DemoItem>();
        int idCounter = 0;

        for (int i = 0; i < count; i++)
        {
            var node = new DemoItem
            {
                Title = $"Root Item {i}",
                Id = ++idCounter,
                Status = i % 2 == 0 ? "Active" : "Closed"
            };

            if (depth > 0)
            {
                for (int j = 0; j < 5; j++)
                {
                    var child = new DemoItem
                    {
                        Title = $"Child {i}-{j}",
                        Id = ++idCounter,
                        Status = "Pending"
                    };
                    node.Children.Add(child);
                }
            }
            list.Add(node);
        }
        return list;
    }
}
