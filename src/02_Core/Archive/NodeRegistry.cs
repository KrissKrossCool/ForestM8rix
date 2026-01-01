using System;
using System.Collections;
using System.Collections.Generic;

namespace ForestM8rix.StateManagement22;

public class NodeRegistry : IDisposable
{
    private readonly List<object> _nodesList = new();
    private readonly List<int> _levelsList = new();

    // [NEW] Словарь для мгновенного поиска "Объект -> Его текущая позиция в списке"
    private Dictionary<object, int> _visualIndexMap = new();

    public object[] Nodes { get; private set; } = Array.Empty<object>();
    public int[] Levels { get; private set; } = Array.Empty<int>();

    // [NEW] Публичный доступ к словарю
    public IReadOnlyDictionary<object, int> VisualIndexMap => _visualIndexMap;

    public int Count => Nodes.Length;

    // Основной метод перестройки плоского списка
    public void Process(IEnumerable source, Func<object, IEnumerable> childSelector)
    {
        // 1. Очищаем всё перед новой сборкой
        _nodesList.Clear();
        _levelsList.Clear();
        _visualIndexMap.Clear();

        if (source != null)
        {
            foreach (var item in source)
            {
                BuildFlatList(item, 0, childSelector);
            }
        }

        // 2. Копируем в публичные массивы
        Nodes = _nodesList.ToArray();
        Levels = _levelsList.ToArray();
    }

    private void BuildFlatList(object node, int level, Func<object, IEnumerable> childSelector)
    {
        if (node == null) return;

        // 3. Заполняем и список, и словарь
        _nodesList.Add(node);
        _levelsList.Add(level);

        // Текущий индекс = последняя позиция в списке
        _visualIndexMap[node] = _nodesList.Count - 1;

        var children = childSelector?.Invoke(node);
        if (children != null)
        {
            foreach (var child in children)
            {
                BuildFlatList(child, level + 1, childSelector);
            }
        }
    }

    public void Dispose()
    {
        _nodesList.Clear();
        _levelsList.Clear();
        _visualIndexMap.Clear();

        Nodes = Array.Empty<object>();
        Levels = Array.Empty<int>();
    }
}
