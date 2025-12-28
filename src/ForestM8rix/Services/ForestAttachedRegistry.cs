using System;
using System.Collections.Generic;
using System.Text;

namespace ForestM8rix.Services;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


// 1. [СУТЬ] Наш реестр "наклеек" (Attached Properties для POCO)
public static class ForestAttachedRegistry
    {
        private static readonly ConditionalWeakTable<object, NodeState> _registry = new();

        public static NodeState GetState(object node)
        {
            if (node == null) return null;
            return _registry.GetValue(node, _ => new NodeState());
        }

        // Класс-контейнер для состояний. Весит мало, создается только при доступе.
        public class NodeState
        {
            public string Id { get; } = Guid.NewGuid().ToString("N");
            public bool IsSelected { get; set; }
            public bool IsExpanded { get; set; }
            public bool? IsChecked { get; set; }
        }
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