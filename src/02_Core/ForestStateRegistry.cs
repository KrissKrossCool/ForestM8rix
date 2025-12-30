using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ForestM8rix.StateManagement
{
    public static class ForestStateRegistry
    {
        private const int CELL_SIZE = 64;

        private static List<BitArray> _selectionLayer = new();
        private static List<BitArray> _expansionLayer = new();
        private static List<BitArray> _visibleLayer = new();

        private class StateIndex
        {
            public readonly uint Value;
            public StateIndex(uint v) => Value = v;
        }

        private static ConditionalWeakTable<object, StateIndex> _indexMap = new();

        public static uint StartIndex => 0;
        private static int _globalCounter = -1;
        // Возвращаем 0, если еще ничего не было, иначе - реальный индекс
        public static uint EndIndex => _globalCounter < 0 ? 0 : (uint)_globalCounter;


        // [NEW] Метод для полного сброса состояния
        public static void Reset()
        {
            lock (_selectionLayer) // Используем один лок для всех, т.к. операция атомарная
            {
                _globalCounter = -1;
                _indexMap = new ConditionalWeakTable<object, StateIndex>();
                _selectionLayer.Clear();
                _expansionLayer.Clear();
                _visibleLayer.Clear();
            }
        }

        public static uint GetOrAssignIndex(object node)
        {
            // Если узел null, возвращаем "невалидный" ID
            if (node == null) return uint.MaxValue;

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

        public static void Toggle(object node)
        {
            bool current = IsExpanded(node);
            SetExpanded(node, !current);
        }

        public static void SelectAll()
        {
            SetAllBitsInRange(_selectionLayer, StartIndex, EndIndex, true);
        }

        public static void ClearSelection()
        {
            SetAllBitsInRange(_selectionLayer, StartIndex, EndIndex, false);
        }

        private static bool GetBit(object node, List<BitArray> layer)
        {
            if (node == null) return false;
            uint id = GetOrAssignIndex(node);
            if (id == uint.MaxValue) return false;

            int cellIdx = (int)(id / CELL_SIZE);

            lock (layer)
            {
                if (cellIdx >= layer.Count) return false;
                return layer[cellIdx].Get((int)(id % CELL_SIZE));
            }
        }

        private static void SetBit(object node, List<BitArray> layer, bool val)
        {
            if (node == null) return;
            uint id = GetOrAssignIndex(node);
            if (id == uint.MaxValue) return;

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

        private static void SetAllBitsInRange(List<BitArray> layer, uint startId, uint endId, bool value)
        {
            if (endId == uint.MaxValue || endId < startId) return;

            lock (layer)
            {
                int requiredCells = (int)(endId / CELL_SIZE) + 1;
                while (layer.Count < requiredCells)
                {
                    layer.Add(new BitArray(CELL_SIZE));
                }

                for (int i = 0; i < layer.Count; i++)
                {
                    layer[i].SetAll(value);
                }
            }
        }
        #endregion
    }
}
