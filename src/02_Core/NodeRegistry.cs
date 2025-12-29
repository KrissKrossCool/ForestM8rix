using System;
using System.Collections;
using System.Collections.Generic;

namespace ForestM8rix
{
    public class NodeRegistry : IDisposable
    {
        private readonly List<object> _nodesList = new();
        private readonly List<int> _levelsList = new();

        public object[] Nodes { get; private set; } = Array.Empty<object>();
        public int[] Levels { get; private set; } = Array.Empty<int>();
        public int Count => Nodes.Length;

        public void Process(IEnumerable source, Func<object, IEnumerable> childSelector)
        {
            _nodesList.Clear();
            _levelsList.Clear();

            if (source != null)
            {
                foreach (var item in source)
                {
                    BuildFlatList(item, 0, childSelector);
                }
            }

            Nodes = _nodesList.ToArray();
            Levels = _levelsList.ToArray();
        }

        private void BuildFlatList(object node, int level, Func<object, IEnumerable> childSelector)
        {
            if (node == null) return;

            _nodesList.Add(node);
            _levelsList.Add(level);

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
            Nodes = Array.Empty<object>();
            Levels = Array.Empty<int>();
        }
    }
}