namespace ForestM8rix.Core.Services;

using ForestM8rix.Services;
using System.ComponentModel;

// 2. [СУТЬ] Прокси-узел для SharpTreeView
// Он легкий: хранит только ссылку на данные и делегирует всё "наклейке"
public class ForestNodeProxy : INotifyPropertyChanged
    {
        private readonly object _data;

        public ForestNodeProxy(object data)
        {
            _data = data;
        }

        // [TAG] Системный ID берется из "наклейки"
        public string Id => ForestAttachedRegistry.GetState(_data).Id;

        public bool IsSelected
        {
            get => ForestAttachedRegistry.GetState(_data).IsSelected;
            set
            {
                var state = ForestAttachedRegistry.GetState(_data);
                if (state.IsSelected != value)
                {
                    state.IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsExpanded
        {
            get => ForestAttachedRegistry.GetState(_data).IsExpanded;
            set
            {
                var state = ForestAttachedRegistry.GetState(_data);
                if (state.IsExpanded != value)
                {
                    state.IsExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        // Бизнес-данные (например, имя) берем напрямую из объекта
        public string DisplayText => _data.ToString();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // 3. [СУТЬ] Пример использования (Тест на 1 000 000 элементов)
    //public class ForestM8rixEngine
    //{
    //    public void LoadData()
    //    {
    //        // У нас есть миллион простых строк или POCO
    //        var rawData = new List<string>();
    //        for (int i = 0; i < 1000000; i++) rawData.Add($"Node {i}");

    //        // Создаем прокси только для того, что нужно отобразить
    //        // Благодаря ConditionalWeakTable, ID создастся только в момент обращения
    //        var firstNode = new ForestNodeProxy(rawData[0]);

    //        Console.WriteLine($"Node ID: {firstNode.Id}");
    //        firstNode.IsSelected = true;
    //    }
    //}