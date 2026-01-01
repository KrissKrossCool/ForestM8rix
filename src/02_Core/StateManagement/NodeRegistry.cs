using System;
using System.Collections;
using System.Collections.Generic;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Core;

/// <summary>
/// Реестр плоской структуры дерева. Отвечает за трансформацию иерархии в линейные массивы.
/// </summary>
public partial class NodeRegistry
{

    private readonly List<object> _nodesList = new();
    private readonly List<int> _levelsList = new();
    // [TAG] ДАННЫЕ
    public object[] Nodes { get; private set; } = Array.Empty<object>();
    public int[] Levels { get; private set; } = Array.Empty<int>();

    /// <summary>
    /// Версия данных. Инкрементируется при каждом изменении структуры (фильтр, expansion).
    /// </summary>
    public long Version { get; private set; }
    public int Count => Nodes.Length;

    // Внутренний кэш индексов для быстрого поиска по объекту
    private readonly Dictionary<object, int> _indexCache = new();

    public void Process2(IEnumerable rootSource, Func<object, IEnumerable> childSelector)
    {

        System.Diagnostics.Debug.WriteLine($"[REGISTRY] Process. Source is null: {rootSource == null}");
        _nodesList.Clear();
        _levelsList.Clear();
        _indexCache.Clear();
        // Проверка: видим ли мы элементы в корне?
        int rootCount = 0;

        if (rootSource != null)
        {
            foreach (var item in rootSource)
            {
                rootCount++;
                BuildFlatList(item, 0, _nodesList, _levelsList, childSelector);
            }
        }

        Nodes = _nodesList.ToArray();
        Levels = _levelsList.ToArray();

        // Обновляем кэш индексов
        for (int i = 0; i < Nodes.Length; i++) _indexCache[Nodes[i]] = i;

        Version++; // Сигнал сервисам (например, Selection), что индексы могли измениться
        System.Diagnostics.Debug.WriteLine($"[REGISTRY] Processed {rootCount} root items. Final Nodes: {_nodesList.Count}");
    }

    public void Process(IEnumerable rootSource, Func<object, IEnumerable> childSelector)
    {
        // [TAG] DEBUG_START
        System.Diagnostics.Debug.WriteLine("--- REGISTRY PROCESS START ---");

        if (rootSource == null)
        {
            System.Diagnostics.Debug.WriteLine("[REGISTRY] !!! CRITICAL: rootSource is NULL");
            return;
        }

        // Проверяем реальный тип и количество элементов через приведение
        var collection = rootSource as System.Collections.ICollection;
        int estimatedCount = collection?.Count ?? -1;
        System.Diagnostics.Debug.WriteLine($"[REGISTRY] Source Type: {rootSource.GetType().Name}, Estimated Count: {estimatedCount}");

        var nodesList = new List<object>();
        var levelsList = new List<int>();
        _indexCache.Clear();

        int rootCount = 0;

        // Пытаемся получить итератор вручную для проверки
        var enumerator = rootSource.GetEnumerator();
        bool hasData = enumerator.MoveNext();
        System.Diagnostics.Debug.WriteLine($"[REGISTRY] Enumerator has data: {hasData}");

        if (hasData)
        {
            // Сбрасываем и проходим циклом
            // (Если это массив, foreach сработает штатно)
            foreach (var item in rootSource)
            {
                rootCount++;
                BuildFlatList(item, 0, nodesList, levelsList, childSelector);
            }
        }

        // [TAG] DATA_SYNC
        Nodes = nodesList.ToArray();
        Levels = levelsList.ToArray();

        for (int i = 0; i < Nodes.Length; i++) _indexCache[Nodes[i]] = i;

        Version++;
        System.Diagnostics.Debug.WriteLine($"[REGISTRY] Root processed: {rootCount}, Final Nodes: {Nodes.Length}");
        System.Diagnostics.Debug.WriteLine("--- REGISTRY PROCESS END ---");
    }

    private void BuildFlatList(object node, int level, List<object> nodes, List<int> levels, Func<object, IEnumerable> selector)
    {
        if (node == null) return;

        // 3. Заполняем и список, и словарь
        nodes.Add(node);
        levels.Add(level);

        string name;
        if (node is null) name = "null";
        else name = node.ToString();

        //bool expanded = Expansion.IsExpanded(node);

        System.Diagnostics.Debug.WriteLine($"[REGISTRY] Add: {name} | Level: {level} ");

        var children = selector?.Invoke(node);
        if (children != null)
        {
            foreach (var child in children)
            {
                BuildFlatList(child, level + 1, nodes, levels, selector);
            }
        }
        System.Diagnostics.Debug.WriteLine($"[REGISTRY] BuildFlatList");
    }

    public int GetVisualIndex(object node)
    {
        if (node == null) return -1;
        return _indexCache.TryGetValue(node, out int idx) ? idx : -1;
    }

    // --- ЛОГИКА СВЯЗЕЙ (ДЛЯ ЛИНИЙ ДЕРЕВА) ---

    public bool IsLastChild(int idx)
    {
        if (idx >= Count - 1) return true;

        int currentLevel = Levels[idx];
        // Используем ForestStateRegistry для получения конца ветки (если реализовано)
        // Иначе — быстрый линейный поиск соседа на том же уровне
        for (int i = idx + 1; i < Count; i++)
        {
            if (Levels[i] == currentLevel) return false; // Есть брат ниже
            if (Levels[i] < currentLevel) return true;   // Вышли к родителю
        }
        return true;
    }

    public bool[] GetParentHierarchyInfo(int idx)
    {
        int level = Levels[idx];
        if (level <= 0) return Array.Empty<bool>();

        bool[] hierarchy = new bool[level];
        for (int L = 0; L < level; L++)
        {
            hierarchy[L] = HasSiblingAtLevel(idx, L);
        }
        return hierarchy;
    }

    private bool HasSiblingAtLevel(int currentIdx, int targetLevel)
    {
        for (int i = currentIdx + 1; i < Count; i++)
        {
            if (Levels[i] == targetLevel) return true;
            if (Levels[i] < targetLevel) return false;
        }
        return false;
    }
}