using System;
using ForestM8rix.Core;
using ForestM8rix.StateManagement; 

namespace ForestM8rix.Services
{
    public class ForestM8rixServiceSelection
    {
        private readonly ForestM8rixManager _mgr;

        // [TAG] Умный Якорь
        private struct SelectionAnchor
        {
            public object Node;       // Ссылка на сам объект
            public int CachedIndex;   // Последний известный индекс
            public long Version;      // Версия реестра
        }

        private SelectionAnchor _anchor;
        public bool EnableFocusFrame { get; set; } = true;

        public ForestM8rixServiceSelection(ForestM8rixManager manager)
        {
            _mgr = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public void HandleClick(int currentIndex, bool isControl, bool isShift)
        {
            var nodes = _mgr.Nodes;
            if (currentIndex < 0 || currentIndex >= nodes.Length) return;

            object currentNode = nodes[currentIndex];

            if (isShift && _anchor.Node != null)
            {
                PerformRangeSelection(currentIndex);
            }
            else
            {
                if (!isControl) ForestStateRegistry.ClearSelection();

                bool newState = isControl ? !ForestStateRegistry.IsSelected(currentNode) : true;
                ForestStateRegistry.SetSelected(currentNode, newState);

                UpdateAnchor(currentNode, currentIndex);
            }

            _mgr.RequestRender();
        }

        public void MoveActive(int delta)
        {
            if (_mgr.Count == 0) return;

            int currentIdx = GetActualAnchorIndex();
            if (currentIdx == -1) currentIdx = 0;

            int newIdx = Math.Clamp(currentIdx + delta, 0, _mgr.Count - 1);
            if (newIdx == currentIdx) return;

            object newNode = _mgr.Nodes[newIdx];

            ForestStateRegistry.ClearSelection();
            ForestStateRegistry.SetSelected(newNode, true);
            UpdateAnchor(newNode, newIdx);

            _mgr.Display.EnsureVisible(newIdx);
            _mgr.RequestRender();
        }

        public void SelectAll()
        {
            foreach (var node in _mgr.Nodes) ForestStateRegistry.SetSelected(node, true);
            _mgr.RequestRender();
        }

        // [СУТЬ] Логика Умного Якоря
        public int GetActualAnchorIndex()
        {
            if (_anchor.Node == null) return -1;

            // 1. Прямое совпадение по версии реестра
            if (_anchor.Version == _mgr.RegistryVersion) return _anchor.CachedIndex;

            // 2. Проверка, не остался ли объект на том же месте
            if (_anchor.CachedIndex < _mgr.Count && _mgr.Nodes[_anchor.CachedIndex] == _anchor.Node)
                return _anchor.CachedIndex;

            // 3. Поиск через реестр
            int newIndex = _mgr.GetVisualIndex(_anchor.Node);
            if (newIndex != -1) UpdateAnchor(_anchor.Node, newIndex);

            return newIndex;
        }

        private void UpdateAnchor(object node, int index)
        {
            _anchor = new SelectionAnchor
            {
                Node = node,
                CachedIndex = index,
                Version = _mgr.RegistryVersion
            };
        }

        private void PerformRangeSelection(int targetIndex)
        {
            int anchorIdx = GetActualAnchorIndex();
            if (anchorIdx == -1) return;

            int start = Math.Min(anchorIdx, targetIndex);
            int end = Math.Max(anchorIdx, targetIndex);

            ForestStateRegistry.ClearSelection();
            for (int i = start; i <= end; i++)
            {
                ForestStateRegistry.SetSelected(_mgr.Nodes[i], true);
            }
        }

        public bool ShouldDrawFocusFrame(int index) => EnableFocusFrame && IsActiveAnchor(index);

        private bool IsActiveAnchor(int index)
        {
            if (_anchor.Node == null) return false;
            return GetActualAnchorIndex() == index;
        }
    }
}