// [ПОЛНЫЙ]
using System.Collections.Generic;
using System.Windows;
using ForestM8rix.Columns;

namespace ForestM8rix.WPF.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitMatrix();
    }

    private void InitMatrix()
    {
        // 1. Настраиваем колонки
        var colName = new ForestM8rixColumn
        {
            Header = "Название",
            BindingPath = "Name",
            Width = 250
        };

        var colType = new ForestM8rixColumn
        {
            Header = "Тип",
            BindingPath = "Type",
            Width = 100
        };

        MatrixView.Manager.Column.Add(colName);
        MatrixView.Manager.Column.Add(colType);

        // 2. Генерируем тестовые данные
        var data = GenerateTestData();

        // 3. Устанавливаем источник данных и селектор дочерних элементов
        MatrixView.Manager.SetItemsSource(data, item => ((FileSystemItem)item).Children);


       
        //MatrixView.Manager.SetItemsSource(data, item =>
        //{
        //    var fsItem = item as FileSystemItem;
        //    return fsItem?.Children ?? new List<FileSystemItem>(); // Возвращаем пустой список вместо null
        //});
    }

    private List<FileSystemItem> GenerateTestData()
    {
        var root = new FileSystemItem("Project", "Folder");

        var src = new FileSystemItem("Source", "Folder");
        src.Children.Add(new FileSystemItem("Main.cs", "File", 1024));
        src.Children.Add(new FileSystemItem("Utils.cs", "File", 2048));

        var docs = new FileSystemItem("Docs", "Folder");
        docs.Children.Add(new FileSystemItem("Readme.md", "File", 512));

        // Глубокая вложенность для проверки линий
        var subFolder = new FileSystemItem("Deep", "Folder");
        subFolder.Children.Add(new FileSystemItem("Secret.txt", "File", 7));
        docs.Children.Add(subFolder);

        root.Children.Add(src);
        root.Children.Add(docs);
        root.Children.Add(new FileSystemItem("App.config", "File", 128));

        return new List<FileSystemItem> { root };
    }
}

public class FileSystemItem
{
    public string Name { get; set; }
    public string Type { get; set; }
    public long Size { get; set; }
    public List<FileSystemItem> Children { get; set; } = new();

    public FileSystemItem(string name, string type, long size = 0)
    {
        Name = name;
        Type = type;
        Size = size;
    }

    public override string ToString()
    {
        return "1. Name: " + Name + "   2. Type: " + Type;
    }
}