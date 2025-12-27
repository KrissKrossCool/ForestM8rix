using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Threading;

namespace ForestM8rix.Core.StateManagement;

/// <summary>
/// Реестр состояний "Соты". 
/// Хранит данные вне объектов в сегментированной битовой сетке.
/// </summary>
public static class ForestStateRegistry
{
    // Размер сегмента (соты). 64 бита — оптимально для 64-битных систем.
    private const int CELL_SIZE = 64;

    // Матрица состояний: Слои (Selection, Expansion)
    private static readonly List<BitArray> _selectionLayer = new();
    private static readonly List<BitArray> _expansionLayer = new();

    // [TAG] Индекс-паспорт: неизменяемый (readonly) и только положительный (uint)
    private class StateIndex
    {
        public readonly uint Value;
        public StateIndex(uint v) => Value = v;
    }

    // Таблица-клей для связи POCO с их индексами
    private static readonly ConditionalWeakTable<object, StateIndex> _indexMap = new();

    // Атомарный счетчик (используем int для совместимости с Interlocked, но храним как uint)
    private static int _globalCounter = -1;

    /// <summary>
    /// Получает или назначает уникальный системный индекс для объекта.
    /// </summary>
    public static uint GetOrAssignIndex(object node)
    {
        return _indexMap.GetValue(node, _ =>
        {
            // Безопасное инкрементирование в многопоточной среде
            uint newId = (uint)Interlocked.Increment(ref _globalCounter);
            return new StateIndex(newId);
        }).Value;
    }

    #region API управления состояниями

    public static bool IsSelected(object node) => GetBit(node, _selectionLayer);
    public static void SetSelected(object node, bool val) => SetBit(node, _selectionLayer, val);

    public static bool IsExpanded(object node) => GetBit(node, _expansionLayer);
    public static void SetExpanded(object node, bool val) => SetBit(node, _expansionLayer, val);

    private static bool GetBit(object node, List<BitArray> layer)
    {
        uint id = GetOrAssignIndex(node);
        int cellIdx = (int)(id / CELL_SIZE);

        lock (layer) // Потокобезопасность при чтении/записи в List
        {
            if (cellIdx >= layer.Count) return false;
            return layer[cellIdx].Get((int)(id % CELL_SIZE));
        }
    }

    private static void SetBit(object node, List<BitArray> layer, bool val)
    {
        uint id = GetOrAssignIndex(node);
        int cellIdx = (int)(id / CELL_SIZE);

        lock (layer)
        {
            // Наращиваем соты (ряды) по мере необходимости
            while (layer.Count <= cellIdx)
            {
                layer.Add(new BitArray(CELL_SIZE));
            }
            layer[cellIdx].Set((int)(id % CELL_SIZE), val);
        }
    }
    #endregion
}

/// <summary>
/// Облегченный прокси-узел для WPF. 
/// Создается только для видимых в UI элементов (виртуализация).
/// </summary>
public class ForestNodeProxy : INotifyPropertyChanged
{
    // Ссылка на исходный POCO-объект данных
    public object RawData { get; }

    public ForestNodeProxy(object data)
    {
        RawData = data ?? throw new ArgumentNullException(nameof(data));
    }

    // Свойства транслируют запросы в ForestStateRegistry
    public bool IsSelected
    {
        get => ForestStateRegistry.IsSelected(RawData);
        set
        {
            if (IsSelected != value)
            {
                ForestStateRegistry.SetSelected(RawData, value);
                Notify();
            }
        }
    }

    public bool IsExpanded
    {
        get => ForestStateRegistry.IsExpanded(RawData);
        set
        {
            if (IsExpanded != value)
            {
                ForestStateRegistry.SetExpanded(RawData, value);
                Notify();
            }
        }
    }

    // Пример отображения текста (можно кастомизировать через шаблоны в XAML)
    public string DisplayName => RawData.ToString();

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    private void Notify([CallerMemberName] string p = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    #endregion
}