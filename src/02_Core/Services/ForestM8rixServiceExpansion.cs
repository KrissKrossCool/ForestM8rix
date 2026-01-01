using System;
using System.Collections;
using ForestM8rix.Core;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Services
{
    public class ForestM8rixServiceExpansion
    {
        private readonly ForestM8rixManager _mgr;

        /// <summary>
        /// Делегат для получения дочерних элементов бизнес-объекта. 
        /// Задается извне при инициализации ItemsSource.
        /// </summary>
        public Func<object, IEnumerable> ChildSelector { get; set; }

        public ForestM8rixServiceExpansion(ForestM8rixManager manager)
        {
            _mgr = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public bool IsExpanded(object node) => ForestStateRegistry.IsExpanded(node);

        // [TAG] ЦЕНТРАЛЬНЫЙ МЕТОД ПЕРЕКЛЮЧЕНИЯ
        public void Toggle(object node)
        {
            if (node == null) return;

            ForestStateRegistry.Toggle(node);

            // После изменения состояния всегда пересобираем плоский список
            _mgr.Refresh();
        }

        /// <summary>
        /// Реализация умной навигации Left/Right.
        /// </summary>
        public void HandleHorizontalNav(int delta, int currentIndex)
        {
            if (currentIndex < 0 || currentIndex >= _mgr.Count) return;

            object node = _mgr.Nodes[currentIndex];
            bool expanded = IsExpanded(node);
            bool hasChildren = _mgr.HasChildren(node);

            if (delta > 0) // ВПРАВО (Раскрытие или вход внутрь)
            {
                if (hasChildren && !expanded)
                {
                    Toggle(node);
                }
                else if (hasChildren && expanded)
                {
                    // Если уже раскрыто, идем на первого ребенка
                    _mgr.Selection.MoveActive(1);
                }
            }
            else // ВЛЕВО (Сворачивание или прыжок к родителю)
            {
                if (hasChildren && expanded)
                {
                    Toggle(node);
                }
                else
                {
                    JumpToParent(currentIndex);
                }
            }
        }

        private void JumpToParent(int currentIndex)
        {
            int currentLevel = _mgr.Levels[currentIndex];
            if (currentLevel == 0) return; // Мы уже в корне

            // Ищем вверх первый узел, чей уровень на 1 меньше текущего
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                if (_mgr.Levels[i] < currentLevel)
                {
                    // Выделяем родителя без Ctrl/Shift
                    _mgr.Selection.HandleClick(i, false, false);
                    _mgr.Display.EnsureVisible(i);
                    break;
                }
            }
        }

        public void ExpandAll()
        {
            foreach (var node in _mgr.Nodes)
            {
                if (_mgr.HasChildren(node))
                {
                    ForestStateRegistry.SetExpanded(node, true);
                }
            }
            _mgr.Refresh();
        }

        public void CollapseAll()
        {
            foreach (var node in _mgr.Nodes)
            {
                ForestStateRegistry.SetExpanded(node, false);
            }
            _mgr.Refresh();
        }
    }
}