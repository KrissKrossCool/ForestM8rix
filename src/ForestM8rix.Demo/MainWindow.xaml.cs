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
