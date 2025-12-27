# [TAG] Установка кодировки для корректной записи кириллицы (если нужно)
$OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "--- Начинаю развертывание ForestM8rix.Demo ---" -ForegroundColor Cyan

# 1. Переходим в папку src (скрипт должен лежать в корне решения или в src)
if (Test-Path "src") { cd src }

# 2. Создание проекта, если его нет, и добавление ссылок
if (-not (Test-Path "ForestM8rix.Demo")) {
    dotnet new wpf -n ForestM8rix.Demo
    dotnet sln ../ForestM8rix.sln add ForestM8rix.Demo/ForestM8rix.Demo.csproj
    dotnet add ForestM8rix.Demo/ForestM8rix.Demo.csproj reference Core/Core.csproj
    
    # ПРИМЕЧАНИЕ: Здесь предполагается, что пакет SharpTreeView доступен через NuGet
    dotnet add ForestM8rix.Demo/ForestM8rix.Demo.csproj package ICSharpCode.TreeView
}

# 3. Запись файла модели (Адаптер)
$assemblyViewModelContent = @"
using ForestM8rix.Core.Interfaces;
using ICSharpCode.TreeView;
using System.Collections.Generic;
using System.Linq;

namespace ForestM8rix.Demo
{
    public class AssemblyViewModel : SharpTreeNode, IForestM8rixNode
    {
        private bool _isVisible = true;

        string IForestM8rixNode.DisplayName => this.Text?.ToString() ?? string.Empty;

        IEnumerable<IForestM8rixNode>? IForestM8rixNode.Children => this.Children.Cast<IForestM8rixNode>();

        public override bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
            }
        }

        bool IForestM8rixNode.IsExpanded
        {
            get => this.IsExpanded;
            set => this.IsExpanded = value;
        }

        public AssemblyViewModel(string title)
        {
            this.Text = title;
        }
    }
}
"@
Set-Content -Path "ForestM8rix.Demo/AssemblyViewModel.cs" -Value $assemblyViewModelContent

# 4. Запись MainWindow.xaml (Разметка)
$xamlContent = @"
<Window x:Class="ForestM8rix.Demo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:tv="clr-namespace:ICSharpCode.TreeView;assembly=ICSharpCode.TreeView"
        Title="ForestM8rix Speed Demo" Height="450" Width="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <StackPanel Margin="10">
            <TextBlock Text="Поиск (миллион узлов):" FontWeight="Bold"/>
            <TextBox x:Name="SearchBox" TextChanged="SearchBox_TextChanged" Margin="0,5,0,0"/>
        </StackPanel>

        <tv:SharpTreeView x:Name="MyTree" Grid.Row="1" Margin="10" ShowRoot="False"/>
    </Grid>
</Window>
"@
Set-Content -Path "ForestM8rix.Demo/MainWindow.xaml" -Value $xamlContent

# 5. Запись MainWindow.xaml.cs (Логика и Генератор данных)
$codeBehindContent = @"
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using ForestM8rix.Core.Services;
using ForestM8rix.Core.Interfaces;

namespace ForestM8rix.Demo
{
    public partial class MainWindow : Window
    {
        private ForestFilterService _filterService = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            var rootNode = new AssemblyViewModel("Hidden Root");
            
            // Генерируем тестовое дерево (1000 x 1000 = 1,000,000)
            for (int i = 0; i < 1000; i++)
            {
                var parent = new AssemblyViewModel($"Folder {i}");
                for (int j = 0; j < 1000; j++)
                {
                    parent.Children.Add(new AssemblyViewModel($"File {i}-{j}"));
                }
                rootNode.Children.Add(parent);
            }

            MyTree.Root = rootNode;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = (sender as TextBox).Text;
            var roots = MyTree.Root.Children.Cast<IForestM8rixNode>();
            
            // Наш сверхбыстрый движок
            _filterService.ApplyFilter(roots, query);
        }
    }
}
"@
Set-Content -Path "ForestM8rix.Demo/MainWindow.xaml.cs" -Value $codeBehindContent

Write-Host "--- Готово! Проект ForestM8rix.Demo обновлен ---" -ForegroundColor Green