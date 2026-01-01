using ForestM8rix.Services;
using ForestM8rix.StateManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ForestM8rix.Core
{
    /// <summary>
    /// Оркестратор системы. Связывает данные, визуализацию и пользовательский ввод.
    /// </summary>
    public partial class ForestM8rixManager
    {
        private readonly FrameworkElement _host;
        private readonly NodeRegistry _registry = new();

        public Typeface DefaultTypeface { get; private set; }
        public double Dpi { get; private set; } = 1.0;

        // [TAG] СЕРВИСЫ
        public ForestM8rixServiceSelection Selection { get; }
        public ForestM8rixServiceDisplay Display { get; }
        public ForestM8rixServiceExpansion Expansion { get; }
        public ForestM8rixServiceColumn Column { get; } // Используем Ваш тип сервиса колонок

        // [TAG] ДОСТУП К ДАННЫМ И СОСТОЯНИЮ
        public object[] Nodes => _registry.Nodes;
        public int[] Levels => _registry.Levels;
        public int Count => _registry.Count;
        public long RegistryVersion => _registry.Version;
        public FrameworkElement View => _host;

        // Глобальные настройки
        public double Scale { get; set; } = 1.0;
        public double IndentSize { get; set; } = 20.0;

        public ForestM8rixManager(FrameworkElement host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));

            // Инициализация сервисов с передачей 'this' [cite: 2025-12-31]
            Selection = new ForestM8rixServiceSelection(this);
            Display = new ForestM8rixServiceDisplay(this);
            Expansion = new ForestM8rixServiceExpansion(this);
            Column = new ForestM8rixServiceColumn(this);

            UpdateVisualParams();
        }

        /// <summary>
        /// Обновляет параметры шрифтов и DPI на основе настроек View.
        /// </summary>
        public void UpdateVisualParams()
        {
            if (View == null) return;

            if (View is System.Windows.Controls.Control control)
            {
                DefaultTypeface = new Typeface(
                    control.FontFamily,
                    control.FontStyle,
                    control.FontWeight,
                    control.FontStretch);
            }
            else
            {
                DefaultTypeface = new Typeface("Segoe UI");
            }

            Dpi = VisualTreeHelper.GetDpi(View).PixelsPerDip;
            RequestRender();
        }

        // --- УПРАВЛЕНИЕ ДАННЫМИ ---
        private IEnumerable _rawSource;

        public void SetItemsSource(IEnumerable items, Func<object, IEnumerable> selector)
        {
            _rawSource = items?.Cast<object>().ToArray();

            Display.VerticalOffset = 0;
            Expansion.ChildSelector = selector;
            Refresh(_rawSource);
        }

        public void Refresh(IEnumerable source = null)
        {
            if (source != null) _rawSource = source;
            var root = _rawSource;

            if (root == null)
            {
                System.Diagnostics.Debug.WriteLine("[MGR] Refresh ABORTED: _rawSource is null");
                return;
            }

            // [TAG] REGISTRY_SYNC: Обновляем плоский список через реестр
            _registry.Process(root, node =>
                Expansion.IsExpanded(node) ? Expansion.ChildSelector?.Invoke(node) : null);

            Display.UpdateMetrics();
            RequestRender();
        }

        public void RequestRender() => _host.InvalidateVisual();

        // --- ПРОБРОС СОБЫТИЙ (ОРКЕСТРАЦИЯ) ---

        public void HandleMouseDown(Point pos, bool ctrl, bool shift)
        {
            int idx = Display.GetIndexFromY(pos.Y);
            if (idx == -1) return;

            if (Display.IsExpanderHit(idx, pos.X))
            {
                Expansion.Toggle(Nodes[idx]);
                this.Refresh();
                System.Diagnostics.Debug.WriteLine($"[MGR] After Toggle: Nodes count = {Nodes.Length}");
            }
            else
            {
                Selection.HandleClick(idx, ctrl, shift);
                RequestRender(); // Для отрисовки рамки выделения
            }
        }

        public void HandleKeyDown(Key key, ModifierKeys modifiers)
        {
            int currentIdx = Selection.GetActualAnchorIndex();
            bool isCtrl = modifiers.HasFlag(ModifierKeys.Control);

            switch (key)
            {
                case Key.Up: Selection.MoveActive(-1); break;
                case Key.Down: Selection.MoveActive(1); break;
                case Key.Left: if (currentIdx != -1) Expansion.HandleHorizontalNav(-1, currentIdx); break;
                case Key.Right: if (currentIdx != -1) Expansion.HandleHorizontalNav(1, currentIdx); break;
                case Key.A when isCtrl: Selection.SelectAll(); break;
            }
        }

        public void HandleScroll(double vOffset, double hOffset)
        {
            Display.VerticalOffset = vOffset;
            Display.HorizontalOffset = hOffset;
            RequestRender();
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ СЕРВИСОВ ---

        public bool IsLastChild(int idx) => _registry.IsLastChild(idx);
        public bool[] GetParentHierarchyInfo(int idx) => _registry.GetParentHierarchyInfo(idx);
        public int GetVisualIndex(object node) => _registry.GetVisualIndex(node);

        public bool HasChildren(object node)
        {
            var children = Expansion.ChildSelector?.Invoke(node);
            if (children == null) return false;
            var en = children.GetEnumerator();
            return en.MoveNext();
        }
    }
}