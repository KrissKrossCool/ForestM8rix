using System.Collections;
using System.Runtime.CompilerServices;



namespace ForestM8rix.StateManagement;

public static class ForestStateRegistry
{
    private const int CELL_SIZE = 64;

    private static readonly List<BitArray> _selectionLayer = new();
    private static readonly List<BitArray> _expansionLayer = new();
    private static readonly List<BitArray> _visibleLayer = new();

    private class StateIndex
    {
        public readonly uint Value;
        public StateIndex(uint v) => Value = v;
    }

    private static readonly ConditionalWeakTable<object, StateIndex> _indexMap = new();
    private static int _globalCounter = -1;

    public static uint GetOrAssignIndex(object node)
    {
        return _indexMap.GetValue(node, _ =>
        {
            uint newId = (uint)Interlocked.Increment(ref _globalCounter);
            return new StateIndex(newId);
        }).Value;
    }

    #region API

    public static bool IsVisible(object node) => GetBit(node, _visibleLayer);
    public static void SetVisible(object node, bool val) => SetBit(node, _visibleLayer, val);

    public static bool IsSelected(object node) => GetBit(node, _selectionLayer);
    public static void SetSelected(object node, bool val) => SetBit(node, _selectionLayer, val);

    public static bool IsExpanded(object node) => GetBit(node, _expansionLayer);
    public static void SetExpanded(object node, bool val) => SetBit(node, _expansionLayer, val);

    // [СУТЬ] Переключатель состояния (Инверсия текущего значения)
    public static void Toggle(object node)
    {
        // Сначала получаем текущее значение, затем записываем обратное
        bool current = IsExpanded(node);
        SetExpanded(node, !current);
    }

    private static bool GetBit(object node, List<BitArray> layer)
    {
        uint id = GetOrAssignIndex(node);
        int cellIdx = (int)(id / CELL_SIZE);

        lock (layer)
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
            while (layer.Count <= cellIdx)
            {
                layer.Add(new BitArray(CELL_SIZE));
            }
            layer[cellIdx].Set((int)(id % CELL_SIZE), val);
        }
    }


    public static void ClearSelection()
    {
        lock (_selectionLayer)
        {
            // Очищаем каждый BitArray в слое выделения
            foreach (var ba in _selectionLayer)
            {
                ba.SetAll(false);
            }
        }
    }


    #endregion
}

