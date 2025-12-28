using System;
using System.Collections.Generic;
using System.Linq;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Core
{
    public class ForestFilterService
    {
        /// <summary>
        /// Высокопроизводительная фильтрация. 
        /// Результат записывается в BitArray реестра.
        /// </summary>
        public void ApplyFilter(IEnumerable<SharpTreeNode> nodes, string query)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                // 1. Проверка совпадения (быстрый поиск)
                bool isMatch = string.IsNullOrEmpty(query) ||
                               node.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase);

                bool hasVisibleChildren = false;

                // 2. Рекурсия
                if (node.Children != null && node.Children.Count > 0)
                {
                    ApplyFilter(node.Children, query);

                    // Проверяем видимость детей через реестр (быстрее, чем свойство)
                    hasVisibleChildren = node.Children.Any(c => ForestStateRegistry.IsVisible(c));
                }

                // 3. Итоговое состояние видимости
                bool finalVisible = isMatch || hasVisibleChildren;
                ForestStateRegistry.SetVisible(node, finalVisible);

                // 4. Авто-разворачивание при поиске
                if (!string.IsNullOrEmpty(query) && hasVisibleChildren)
                {
                    ForestStateRegistry.SetExpanded(node, true);
                }

                // Уведомляем UI (WPF)
                node.RaisePropertyChanged("IsVisible");
                if (hasVisibleChildren) node.RaisePropertyChanged("IsExpanded");
            }
        }
    }
}