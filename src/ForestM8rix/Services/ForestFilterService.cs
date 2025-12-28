using ForestM8rix.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForestM8rix.Services;

public class ForestFilterService
{
    /// <summary>
    /// [TAG] Высокопроизводительная фильтрация дерева без выделения лишней памяти
    /// </summary>
    public void ApplyFilter(IEnumerable<IForestM8rixNode> nodes, string query)
    {
        foreach (var node in nodes)
        {
            // Проверка совпадения (Ordinal — самый быстрый способ в .NET)
            bool isMatch = string.IsNullOrEmpty(query) ||
                           node.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase);

            if (node.Children != null)
            {
                // Рекурсивный спуск к дочерним элементам
                ApplyFilter(node.Children, query);

                // Узел виден, если он совпал сам ИЛИ виден кто-то из детей
                bool hasVisibleChildren = node.Children.Any(c => c.IsVisible);
                node.IsVisible = isMatch || hasVisibleChildren;

                // Разворачиваем ветку, если внутри найден результат
                if (!string.IsNullOrEmpty(query) && hasVisibleChildren)
                {
                    node.IsExpanded = true;
                }
            }
            else
            {
                node.IsVisible = isMatch;
            }
        }
    }
}