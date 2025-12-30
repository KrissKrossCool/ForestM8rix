using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using ForestM8rix;

namespace ForestM8rix.WPF.Demo
{
    public partial class MainWindow : Window
    {
        private List<DemoItem> _allData; // Ссылка на исходные данные для поиска

        public MainWindow()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("--> [CONSTRUCTOR] MainWindow created.");
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("--> [EVENT] MainWindow_Loaded fired.");
            // 1. Настройка
            var mgr = ForestDisplay.Manager;
            mgr.Columns.Clear();
            mgr.Columns.Add(new ForestColumn("Name", 300, n => (n as DemoItem)?.Title));
            mgr.Columns.Add(new ForestColumn("ID", 100, n => (n as DemoItem)?.Id.ToString()));
            mgr.Columns.Add(new ForestColumn("Status", 150, n => (n as DemoItem)?.Status));

            // 2. Генерация (100k)
            int count = 100000;
            Debug.WriteLine($"Generating {count} items...");
            _allData = GenerateData(count);

            // 3. Загрузка
            ForestDisplay.SetData(_allData, node => ((DemoItem)node).Children);
            Debug.WriteLine("Data Loaded.");
        }

        private void BtnSelectRange_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtStart.Text, out int start)) start = 0;
            if (!int.TryParse(TxtEnd.Text, out int end)) end = 0;

            if (start > end) { var t = start; start = end; end = t; }
            if (end >= _allData.Count) end = _allData.Count - 1;

            // Формируем список объектов для выделения
            var selectionList = new List<object>();
            for (int i = start; i <= end; i++)
            {
                selectionList.Add(_allData[i]);
            }

            Debug.WriteLine($"Selecting range [{start}..{end}] ({selectionList.Count} items)...");

            Stopwatch sw = Stopwatch.StartNew();

            // СУТЬ: Управляем через Attached Property (как будто из Binding)
            SelectionData.SetList(ForestDisplay, selectionList);

            sw.Stop();
            TxtStatus.Text = $"Selected {selectionList.Count} items in {sw.ElapsedMilliseconds} ms";
            Debug.WriteLine($"Done in {sw.ElapsedMilliseconds} ms");
        }

        private void BtnSelectRandom_Click(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            var selectionList = new List<object>();
            for (int i = 0; i < 5000; i++) // 5000 случайных
            {
                int idx = rnd.Next(_allData.Count);
                selectionList.Add(_allData[idx]);
            }

            Debug.WriteLine($"Selecting {selectionList.Count} random items...");
            Stopwatch sw = Stopwatch.StartNew();

            SelectionData.SetList(ForestDisplay, selectionList);

            sw.Stop();
            TxtStatus.Text = $"Random Select ({selectionList.Count}) in {sw.ElapsedMilliseconds} ms";
            Debug.WriteLine($"Done in {sw.ElapsedMilliseconds} ms");
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            SelectionData.SetList(ForestDisplay, null); // Сброс
            TxtStatus.Text = "Selection Cleared";
        }

        // Добавить эти обработчики в класс MainWindow

        // Заменить обработчики BtnUp_Click и BtnDown_Click

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            var logic = ForestDisplay as ScrollLogic;
            if (logic != null)
            {
                // Скроллим на одну видимую страницу вверх
                logic.SetVerticalOffset(logic.VerticalOffset - logic.ViewportHeight);
            }
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            var logic = ForestDisplay as ScrollLogic;
            if (logic != null)
            {
                // Скроллим на одну видимую страницу вниз
                logic.SetVerticalOffset(logic.VerticalOffset + logic.ViewportHeight);
            }
        }


        private void BtnGoPercent_Click(object sender, RoutedEventArgs e)
        {
            var logic = ForestDisplay as ScrollLogic;
            if (logic != null && double.TryParse(TxtPercent.Text, out double percent))
            {
               // logic.ScrollToPercent(percent);
                Debug.WriteLine($"BtnGoPercent_Click {percent}");
            }
        }


        // --- DATA MODEL ---
        public class DemoItem
        {
            public string Title { get; set; }
            public int Id { get; set; }
            public string Status { get; set; }
            public List<DemoItem> Children { get; set; } = new();
        }

        private List<DemoItem> GenerateData(int count)
        {
            var list = new List<DemoItem>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(new DemoItem
                {
                    Title = $"Item {i}",
                    Id = i,
                    Status = i % 2 == 0 ? "Active" : "Idle"
                });
            }
            return list;
        }
    }
}
