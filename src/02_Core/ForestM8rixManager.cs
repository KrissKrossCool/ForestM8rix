using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ForestM8rix
{
    public class ForestM8rixManager
    {
        private readonly NodeRegistry _registry = new();
        private readonly UIElement _host;
        private IEnumerable _source;
        private Func<object, IEnumerable> _childSelector;

        public double RowHeight { get; set; } = 24.0;
        public double VerticalOffset { get; set; } = 0;

        public int Count => _registry.Count;
        public object[] Nodes => _registry.Nodes;
        public int[] Levels => _registry.Levels;

        public ForestM8rixManager(UIElement host)
        {
            _host = host;
        }

        public void SetSource(IEnumerable source, Func<object, IEnumerable> childSelector)
        {
            _source = source;
            _childSelector = childSelector;
            Refresh();
        }

        public void Refresh()
        {
            if (_source == null) return;

            _registry.Process(_source, node =>
            {
                // Узел раскрыт только если он есть в словаре состояний со значением true
                if (StateManagement.ForestStateRegistry.IsExpanded(node))
                {
                    return _childSelector?.Invoke(node);
                }
                return null;
            });

            _host.InvalidateVisual();
        }

        public void HandleClick(Point position)
        {
            int index = (int)((position.Y + VerticalOffset) / RowHeight);
            if (index >= 0 && index < _registry.Count)
            {
                object node = _registry.Nodes[index];
                ToggleExpansion(node);
            }
        }

        private void ToggleExpansion(object node)
        {
            bool currentState = StateManagement.ForestStateRegistry.IsExpanded(node);
            StateManagement.ForestStateRegistry.SetExpanded(node, !currentState);
            Refresh();
        }

        public bool HasChildren(object node)
        {
            if (node == null || _childSelector == null) return false;
            var children = _childSelector(node);
            if (children == null) return false;

            // Проверка через приведение к коллекции или перечислитель
            if (children is ICollection coll) return coll.Count > 0;
            return children.Cast<object>().Any();
        }

        public object GetItemAt(double y, out int index)
        {
            index = (int)((y + VerticalOffset) / RowHeight);
            if (index >= 0 && index < _registry.Count)
            {
                return _registry.Nodes[index];
            }
            return null;
        }
    }
}