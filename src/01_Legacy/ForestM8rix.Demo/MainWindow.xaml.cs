// [ПОЛНЫЙ]
using ForestM8rix.Columns;
using ForestM8rix.Testing;
using ForestM8rix.WPF.Demo;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ForestM8rix.WPF.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetupDemo();
    }

    private void SetupDemo()
    {
        // 1. [TAG] Настройка колонок
        // Используем Ваш класс ForestM8rixColumn
        MyForestView.Manager.Columns.Add(new ForestM8rixColumn
        {
            Header = "Структура проекта",
            BindingPath = "Name",
            Width = 300
        });

        MyForestView.Manager.Columns.Add(new ForestM8rixColumn
        {
            Header = "Информация",
            BindingPath = "Info",
            Width = 250
        });

        // 2. [TAG] Генерация тестовых данных (1000 корней по 3 ребенка)
        var data = new List<FolderNode>();
        for (int i = 1; i <= 1000; i++)
        {
            var root = new FolderNode($"Root Node {i}", $"System info {i}");
            root.Children.Add(new FolderNode($"Child {i}.1", "Sub-resource"));
            root.Children.Add(new FolderNode($"Child {i}.2", "Internal data"));
            root.Children.Add(new FolderNode($"Child {i}.3", "Documentation"));
            data.Add(root);
        }

        // 3. [TAG] Инициализация источника
        // Передаем данные и селектор детей (node => node.Children)
        MyForestView.Manager.SetItemsSource(data, node => ((FolderNode)node).Children);
    }
}

public class FolderNode
{
    public string Name { get; set; }
    public string Info { get; set; }
    public List<FolderNode> Children { get; set; } = new List<FolderNode>();

    public FolderNode(string name, string info)
    {
        Name = name;
        Info = info;
    }
}