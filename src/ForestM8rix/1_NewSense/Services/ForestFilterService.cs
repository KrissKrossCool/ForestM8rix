namespace ForestM8rix.Services
{
    public class ForestFilterService
    {
        /// <summary>
        /// [TAG] Применяет фильтр к коллекции узлов. 
        /// Работает напрямую с SharpTreeNode для максимальной скорости.
        /// </summary>
        public void ApplyFilter(IList<SharpTreeNode> nodes, string query)
        {
            if (nodes == null) return;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is SharpTreeNode node)
                {
                    // 1. Простейший поиск по DisplayName
                    bool isMatch = string.IsNullOrEmpty(query) ||
                                   node.DisplayName.Contains(query, System.StringComparison.OrdinalIgnoreCase);

                    // 2. Рекурсия
                    bool hasVisibleChildren = false;
                    if (node.Children.Count > 0)
                    {
                        ApplyFilter(node.Children, query);

                        // Проверяем результат детей
                        foreach (SharpTreeNode child in node.Children)
                        {
                            if (!child.IsHidden)
                            {
                                hasVisibleChildren = true;
                                break;
                            }
                        }
                    }

                    // 3. Установка состояния в оригинальное поле библиотеки
                    node.IsHidden = !(isMatch || hasVisibleChildren);

                    // 4. Раскрытие веток с результатами
                    if (!string.IsNullOrEmpty(query) && hasVisibleChildren)
                    {
                        node.IsExpanded = true;
                    }
                }
            }
        }
    }
}