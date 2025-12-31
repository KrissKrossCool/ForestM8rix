//using ForestM8rix.Columns;
//using ForestM8rix.Rendering;
//using ForestM8rix.StateManagement;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Media;

//namespace ForestM8rix.Core;

//public class ForestM8rixManager
//{
//    private readonly NodeRegistry _registry = new();
//    private readonly FrameworkElement _host;
//    public FrameworkElement View => _host;
//    private IEnumerable _source;
//    private Func<object, IEnumerable> _childSelector;

//    // Кэшируем DPI для производительности
//    public double Dpi { get; private set; }
//    // Переименовали свойство, чтобы не конфликтовать с типом Typeface
//    public Typeface DefaultTypeface { get; private set; }

//    // --- ГЕОМЕТРИЯ И ЗУМ ---
//    public double Scale { get; set; } = 1.0;
//    public double IndentSize { get; set; } = 20.0;
//    public double RowHeight { get; set; } = 24.0;
//    public double VerticalOffset { get; set; } = 0;
//    public double HorizontalOffset { get; set; } = 0;

//    private double[] _colOffsets;
//    private double _cachedTotalWidth;
//    private readonly Typeface _typeface = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

//    // --- КОЛОНКИ (НОВАЯ ВЕРСИЯ) ---
//    // [TAG] Используем ForestM8rixColumn вместо старого ForestColumn
//    public List<ForestM8rixColumn> Columns { get; } = new();

//    public int Count => _registry.Count;
//    public object[] Nodes => _registry.Nodes;
//    public int[] Levels => _registry.Levels;

//    public bool IsRubberBandEnabled { get; set; }

//    private object _lastSelectedNode;
//            // Находим индекс для будущих операций
//    private int _anchorIndex = 0;
//    private int _lastSelectedIdx = -1;

//    public ForestM8rixManager(FrameworkElement host)
//    {
//        _host = host;
//        UpdateVisualParams();
//    }

//    // Метод для обновления параметров при смене монитора или настроек
//    public void UpdateVisualParams()
//    {
//        Dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;
//        DefaultTypeface = new Typeface(
//            (FontFamily)_host.GetValue(TextElement.FontFamilyProperty),
//            (FontStyle)_host.GetValue(TextElement.FontStyleProperty),
//            (FontWeight)_host.GetValue(TextElement.FontWeightProperty),
//            (FontStretch)_host.GetValue(TextElement.FontStretchProperty));
//    }


//    // [ПОЛНЫЙ]
//    public void OnCanvasMouseDown(Point pos, bool ctrl, bool shift)
//    {
//        double rowH = ForestM8rixOptions.GetRowHeight(View);
//        double sRowH = rowH * Scale;
//        int rowIdx = (int)(pos.Y / sRowH);

//        if (rowIdx < 0 || rowIdx >= Nodes.Length) return;

//        object clickedNode = Nodes[rowIdx];
//        int level = Levels[rowIdx];
//        double indentSize = IndentSize * Scale;
//        double indentX = level * indentSize;

//        // 1. [TAG] ПРОВЕРКА ПОПАДАНИЯ В ЭКСПАНДЕР
//        // Область экспандера такая же, как при отрисовке в RenderRow
//        Rect expanderRect = new Rect(indentX, rowIdx * sRowH, indentSize, sRowH);

//        if (expanderRect.Contains(pos) && HasChildren(clickedNode))
//        {
//            // Инвертируем состояние раскрытия
//            bool isExpanded = ForestStateRegistry.IsExpanded(clickedNode);
//            ForestStateRegistry.SetExpanded(clickedNode, !isExpanded);

//            // Перестраиваем плоский список (Nodes, Levels)
//            RefreshFlatList();

//            View.InvalidateVisual();
//            return; // Выходим, чтобы не менять выделение при клике на экспандер
//        }

//        // 2. [TAG] ЛОГИКА ВЫДЕЛЕНИЯ (ОДИНОЧНЫЙ / MULTI)
//        if (!ctrl && !shift)
//        {
//            ForestStateRegistry.ClearSelection();
//            ForestStateRegistry.SetSelected(clickedNode, true);
//        }
//        else if (ctrl)
//        {
//            bool isSel = ForestStateRegistry.IsSelected(clickedNode);
//            ForestStateRegistry.SetSelected(clickedNode, !isSel);
//        }
//        else if (shift && _lastSelectedIdx != -1)
//        {
//            ForestStateRegistry.ClearSelection();
//            int start = Math.Min(_lastSelectedIdx, rowIdx);
//            int end = Math.Max(_lastSelectedIdx, rowIdx);
//            for (int i = start; i <= end; i++)
//                ForestStateRegistry.SetSelected(Nodes[i], true);
//        }

//        _lastSelectedIdx = rowIdx;

//        // Принудительная перерисовка для мгновенного отклика
//        View.InvalidateVisual();
//    }



//    #region Инициализация и Обновление

//    public void SetSource(IEnumerable source, Func<object, IEnumerable> childSelector)
//    {
//        _source = source;
//        _childSelector = childSelector;
//        Refresh();
//    }

//    public void Refresh()
//    {
//        if (_source == null) return;
//        _registry.Process(_source, node => ForestStateRegistry.IsExpanded(node) ? _childSelector?.Invoke(node) : null);

//        UpdateLayoutMetrics();
//        _host.InvalidateVisual();
//    }



//    // [СУТЬ] -2
//    //public void UpdateLayoutMetrics()
//    //{
//    //    if (Columns == null || Columns.Count == 0) return;

//    //    // 1. Расчет базовой ширины
//    //    double totalW = 0;
//    //    foreach (var col in Columns) totalW += col.Width;
//    //    _cachedTotalWidth = totalW;

//    //    if (_host is ForestM8rixView view)
//    //    {
//    //        double sRowH = RowHeight * Scale;
//    //        double scaledTotalW = _cachedTotalWidth * Scale;
//    //        var canvas = view.GetCanvas();

//    //        if (canvas != null)
//    //        {
//    //            // [TAG] Фикс двойного Scale: используем sRowH (в котором уже есть Scale)
//    //            double contentHeight = Nodes.Length * sRowH;

//    //            // Растягиваем холст минимум на размер окна для красоты фона
//    //            canvas.Height = Math.Max(contentHeight, view.ActualHeight);
//    //            canvas.Width = Math.Max(scaledTotalW, view.ActualWidth);

//    //            canvas.VerticalAlignment = VerticalAlignment.Top;
//    //            canvas.HorizontalAlignment = HorizontalAlignment.Left;

//    //            Canvas.SetLeft(canvas, 0);
//    //            Canvas.SetTop(canvas, 0);
//    //        }
//    //    }
//    //}

//    public void UpdateLayoutMetrics()
//    {
//        if (Columns == null || Columns.Count == 0) return;

//        // 1. Расчет горизонтали
//        double currentX = 0;
//        for (int i = 0; i < Columns.Count; i++)
//        {
//            currentX += Columns[i].Width;
//        }
//        _cachedTotalWidth = currentX;

//        if (_host is ForestM8rixView view)
//        {
//            double sRowH = RowHeight * Scale;
//            var canvas = view.GetCanvas();

//            if (canvas != null)
//            {
//                canvas.Height = Nodes.Length * sRowH * Scale;
//                canvas.Width = _cachedTotalWidth * Scale;

//                canvas.VerticalAlignment = VerticalAlignment.Top;
//                canvas.HorizontalAlignment = HorizontalAlignment.Left;

//                Canvas.SetLeft(canvas, 0);
//                Canvas.SetTop(canvas, 0);
//            }
//        }
//    }

//    public double TotalWidth => _cachedTotalWidth;

//    #endregion

//    #region Рендеринг

//    public void Render(DrawingContext dc, Size renderSize)
//    {
//        if (Nodes == null || Nodes.Length == 0) return;

//        double dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;

//        //double headerHeight = 30;
//        //double headerHeight = (_host as ForestM8rixView)?.HeaderHeight ?? 30.0;

//        double sRowH = RowHeight * Scale;

//        int first = (int)Math.Max(0, Math.Floor(VerticalOffset));
//        int last = (int)Math.Min(Nodes.Length - 1, first + (int)Math.Ceiling(renderSize.Height / sRowH));

//        for (int i = first; i <= last; i++)
//        {
//            //double y = headerHeight + (i - VerticalOffset) * sRowH;
//            // Строка должна рисоваться на своей абсолютной позиции на холсте.
//            //double y = headerHeight + (i * sRowH);
//            double y = i * sRowH;
//            RenderRow(dc, i, y, sRowH, renderSize.Width, dpi);
//        }

//        //RenderHeader(dc, renderSize, headerHeight, dpi);
//    }

//    // [ПОЛНЫЙ]
//    private void RenderRow(DrawingContext dc, int idx, double y, double rowH, double viewW, double dpi)
//    {
//        object node = Nodes[idx];
//        int level = Levels[idx];
//        bool isSelected = ForestStateRegistry.IsSelected(node);

//        // 1. Отрисовка фона строки на ВСЮ ширину вьюпорта
//        Brush rowBg = isSelected
//            ? ForestM8rixOptions.GetSelectionBackground(View)
//            : ForestM8rixOptions.GetNormalBackground(View);

//        if (rowBg != Brushes.Transparent)
//        {
//            // Используем viewW (ActualWidth контрола), чтобы линия шла до края окна
//            dc.DrawRectangle(rowBg, null, new Rect(0, y, viewW, rowH));
//        }

//        double currentX = 0;

//        for (int c = 0; c < Columns.Count; c++)
//        {
//            var col = Columns[c];
//            double colW = col.Width * Scale;

//            // Рисуем колонку, только если она видна
//            if (currentX + colW > 0 && currentX < viewW)
//            {
//                var renderer = col.GetCellRenderer(this);

//                if (c == 0) // Первая колонка: Иерархия + Контент
//                {
//                    double indentSize = IndentSize * Scale;
//                    double indentX = level * indentSize;

//                    ForestRenderHelper.DrawTreeLines(dc, level, currentX, y, rowH, indentSize, IsLastChild(idx), GetParentHierarchyInfo(idx));

//                    if (HasChildren(node))
//                    {
//                        var expCenter = new Point(currentX + indentX + (indentSize / 2), y + rowH / 2);
//                        ForestRenderHelper.DrawExpander(dc, expCenter, ForestStateRegistry.IsExpanded(node), Brushes.DimGray);
//                    }

//                    var iconSize = 16 * Scale;
//                    var iconRect = new Rect(currentX + indentX + indentSize, y + (rowH - iconSize) / 2, iconSize, iconSize);
//                    ForestRenderHelper.DrawIcon(dc, iconRect, col.GetIconKey(node), Brushes.RoyalBlue);

//                    double textOffset = indentX + indentSize + (20 * Scale);
//                    Rect contentRect = new Rect(currentX + textOffset, y, colW - textOffset, rowH);

//                    renderer.Draw(dc, contentRect, node, isSelected);
//                }
//                else // Остальные колонки
//                {
//                    renderer.Draw(dc, new Rect(currentX, y, colW, rowH), node, isSelected);
//                }
//            }
//            currentX += colW;
//        }
//    }

//    //private void RenderRow(DrawingContext dc, int idx, double y, double rowH, double viewW, double dpi)
//    //{
//    //    object node = Nodes[idx];
//    //    int level = Levels[idx];

//    //    // Отрисовка фона (Hover/Selection)
//    //    if (ForestStateRegistry.IsSelected(node))
//    //        dc.DrawRectangle(Brushes.AliceBlue, null, new Rect(0, y, viewW, rowH));

//    //    //double currentX = -HorizontalOffset;
//    //    double currentX = 0;

//    //    for (int c = 0; c < Columns.Count; c++)
//    //    {
//    //        var col = Columns[c];
//    //        double colW = col.Width * Scale;

//    //        if (currentX + colW > 0 && currentX < viewW)
//    //        {
//    //            // [TAG] Получаем текст через навороченную колонку
//    //            string text = col.GetText(node);

//    //            if (c == 0) // Первая колонка с деревом
//    //            {
//    //                double indentX = level * IndentSize * Scale;

//    //                // Линии дерева (передаем информацию о наличии соседей)
//    //                ForestRenderHelper.DrawTreeLines(dc, level, currentX, y, rowH, IndentSize * Scale, IsLastChild(idx), GetParentHierarchyInfo(idx));

//    //                // Экспандер
//    //                if (HasChildren(node))
//    //                {
//    //                    var expCenter = new Point(currentX + indentX + (IndentSize * Scale / 2), y + rowH / 2);
//    //                    ForestRenderHelper.DrawExpander(dc, expCenter, ForestStateRegistry.IsExpanded(node), Brushes.DimGray);
//    //                }

//    //                // Иконка
//    //                var iconRect = new Rect(currentX + indentX + (IndentSize * Scale), y + (rowH - 16 * Scale) / 2, 16 * Scale, 16 * Scale);
//    //                ForestRenderHelper.DrawIcon(dc, iconRect, col.GetIconKey(node), Brushes.RoyalBlue);

//    //                // Текст ячейки
//    //                var textRect = new Rect(currentX + indentX + (IndentSize * Scale) + 20 * Scale, y, colW - (indentX + 40 * Scale), rowH);
//    //                ForestRenderHelper.DrawCellText(dc, text, textRect, _typeface, 12 * Scale, Brushes.Black, dpi);
//    //            }
//    //            else
//    //            {
//    //                ForestRenderHelper.DrawCellText(dc, text, new Rect(currentX, y, colW, rowH), _typeface, 12 * Scale, Brushes.Black, dpi);
//    //            }
//    //        }
//    //        currentX += colW;
//    //    }
//    //}

//    private void RenderHeader(DrawingContext dc, Size size, double h, double dpi)
//    {
//        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(245, 245, 245)), null, new Rect(0, 0, size.Width, h));
//        double cx = -HorizontalOffset;
//        foreach (var col in Columns)
//        {
//            ForestRenderHelper.DrawCellText(dc, col.Header, new Rect(cx, 0, col.Width, h), _typeface, 12, Brushes.DimGray, dpi);
//            cx += col.Width;
//        }
//    }

//    #endregion

//    private bool IsLastChild(int index) => (index >= Nodes.Length - 1) || (Levels[index + 1] < Levels[index]);

//    private bool[] GetParentHierarchyInfo(int index) => new bool[Levels[index]]; // Заглушка для линий

//    private bool HasChildren(object node)
//    {
//        var children = _childSelector?.Invoke(node);
//        if (children is ICollection coll) return coll.Count > 0;
//        return children?.Cast<object>().Any() ?? false;
//    }

//    /// <summary>
//    /// [TAG] Публичный API для управления выделением узлов
//    /// </summary>

//    public void SetSelection(object node, bool isSelected=true, bool clearExisting = false)
//    {
//        if (node == null) return;

//        if (clearExisting)
//        {
//            ForestStateRegistry.ClearSelection();
//        }

//        ForestStateRegistry.SetSelected(node, isSelected);

//        // Запоминаем последний выбранный узел для логики Shift+Click (Anchor)
//        if (isSelected)
//        {
//            _lastSelectedNode = node;
//            // Находим индекс для будущих операций
//            _anchorIndex = Array.IndexOf(Nodes, node);
//        }

//        // Заставляем контрол перерисоваться, чтобы увидеть изменения
//        _host.InvalidateVisual();
//    }

//    /// <summary>
//    /// Выделение диапазона (например, при зажатом Shift)
//    /// </summary>
//    public void SelectRange(int startIndex, int endIndex)
//    {
//        if (Nodes == null) return;

//        int start = Math.Min(startIndex, endIndex);
//        int end = Math.Max(startIndex, endIndex);

//        ForestStateRegistry.ClearSelection();

//        for (int i = start; i <= end; i++)
//        {
//            ForestStateRegistry.SetSelected(Nodes[i], true);
//        }

//        _host.InvalidateVisual();
//    }
//}

// [ПОЛНЫЙ]


//using ForestM8rix.Rendering;
//using ForestM8rix.StateManagement; // Для NodeRegistry
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Windows;
//using System.Windows.Documents;
//using System.Windows.Media;

//namespace ForestM8rix.Core
//{
//    public class ForestM8rixManager
//    {
//        private readonly FrameworkElement _host;
//        private readonly NodeRegistry _registry;
//        private int _lastSelectedIdx = -1;
//        private IEnumerable _rootNodes;

//        // Данные из репозитория (Registry)
//        public object[] Nodes => _registry.Nodes;
//        public int[] Levels => _registry.Levels;

//        // Настройки и визуальные параметры
//        public double Scale { get; set; } = 1.0;
//        public double IndentSize { get; set; } = 20.0;
//        public double Dpi { get; private set; }
//        public Typeface DefaultTypeface { get; private set; }
//        public FrameworkElement View => _host;

//        public ForestM8rixManager(FrameworkElement host, NodeRegistry registry)
//        {
//            _host = host;
//            _registry = registry;
//            UpdateVisualParams();
//        }

//        public void UpdateVisualParams()
//        {
//            Dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;
//            DefaultTypeface = new Typeface(
//                (FontFamily)_host.GetValue(TextElement.FontFamilyProperty),
//                (FontStyle)_host.GetValue(TextElement.FontStyleProperty),
//                (FontWeight)_host.GetValue(TextElement.FontWeightProperty),
//                (FontStretch)_host.GetValue(TextElement.FontStretchProperty));
//        }

//        // Установка исходных данных
//        public void SetItemsSource(IEnumerable items)
//        {
//            _rootNodes = items;
//            RefreshFlatList();
//        }

//        public void RefreshFlatList()
//        {
//            // Используем встроенный механизм NodeRegistry для построения списка
//            _registry.Process(_rootNodes, GetExpandedChildren);
//            _host.InvalidateVisual();
//        }


//        public void OnCanvasMouseDown(Point pos, bool ctrl, bool shift)
//        {
//            double rowH = ForestM8rixOptions.GetRowHeight(_host) * Scale;
//            int rowIdx = (int)(pos.Y / rowH);

//            if (rowIdx < 0 || rowIdx >= Nodes.Length) return;

//            object clickedNode = Nodes[rowIdx];
//            double indentX = Levels[rowIdx] * IndentSize * Scale;

//            // 1. Проверка клика по экспандеру
//            Rect expRect = new Rect(indentX, rowIdx * rowH, IndentSize * Scale, rowH);
//            if (expRect.Contains(pos) && HasChildren(clickedNode))
//            {
//                // Используем Ваш метод Toggle
//                ForestStateRegistry.Toggle(clickedNode);
//                RefreshFlatList();
//                return;
//            }

//            // 2. Логика выделения
//            if (!ctrl && !shift)
//                ForestStateRegistry.ClearSelection();

//            ForestStateRegistry.SetSelected(clickedNode, true);
//            _lastSelectedIdx = rowIdx;

//            _host.InvalidateVisual();
//        }

//        public void RenderRow(DrawingContext dc, int idx, double y, double rowH, double viewW, double dpi, List<IForestColumn> columns)
//        {
//            object node = Nodes[idx];
//            int level = Levels[idx];
//            bool isSelected = ForestStateRegistry.IsSelected(node);

//            // Отрисовка фона строки на всю ширину окна
//            Brush rowBg = isSelected
//                ? ForestM8rixOptions.GetSelectionBackground(_host)
//                : ForestM8rixOptions.GetNormalBackground(_host);

//            if (rowBg != Brushes.Transparent)
//                dc.DrawRectangle(rowBg, null, new Rect(0, y, viewW, rowH));

//            double currentX = 0;
//            for (int i = 0; i < columns.Count; i++)
//            {
//                var col = columns[i];
//                double colW = col.Width * Scale;

//                if (currentX + colW > 0 && currentX < viewW)
//                {
//                    var renderer = col.GetCellRenderer(this);

//                    if (i == 0) // Первая колонка с иерархией
//                    {
//                        double sIndent = IndentSize * Scale;
//                        double indentX = level * sIndent;

//                        // [TAG] Визуализация структуры дерева
//                        ForestRenderHelper.DrawTreeLines(dc, level, currentX, y, rowH, sIndent, IsLastChild(idx), GetParentHierarchyInfo(idx));

//                        if (HasChildren(node))
//                        {
//                            var expCenter = new Point(currentX + indentX + (sIndent / 2), y + rowH / 2);
//                            ForestRenderHelper.DrawExpander(dc, expCenter, ForestStateRegistry.IsExpanded(node), Brushes.DimGray);
//                        }

//                        var iconRect = new Rect(currentX + indentX + sIndent, y + (rowH - 16 * Scale) / 2, 16 * Scale, 16 * Scale);
//                        ForestRenderHelper.DrawIcon(dc, iconRect, col.GetIconKey(node), Brushes.RoyalBlue);

//                        double textOffset = indentX + sIndent + (20 * Scale);
//                        renderer.Draw(dc, new Rect(currentX + textOffset, y, colW - textOffset, rowH), node, isSelected);
//                    }
//                    else // Обычные колонки
//                    {
//                        renderer.Draw(dc, new Rect(currentX, y, colW, rowH), node, isSelected);
//                    }
//                }
//                currentX += colW;
//            }
//        }

//        //private bool HasChildren(object node)
//        //    => node is ForestM8rix.FolderNode folder && folder.Children?.Count > 0;

//        // [СУТЬ]
//        private IEnumerable GetExpandedChildren(object node)
//        {
//            if (ForestStateRegistry.IsExpanded(node))
//            {
//                // Пытаемся найти свойство Children у любого объекта
//                var property = node.GetType().GetProperty("Children")
//                            ?? node.GetType().GetProperty("Items");

//                if (property != null)
//                    return property.GetValue(node) as IEnumerable;
//            }
//            return null;
//        }

//        private bool HasChildren(object node)
//        {
//            if (node == null) return false;

//            // Ищем свойство Children или Items
//            var property = node.GetType().GetProperty("Children")
//                        ?? node.GetType().GetProperty("Items");

//            if (property != null)
//            {
//                var value = property.GetValue(node) as IEnumerable;
//                // Проверяем, что в списке есть хотя бы один элемент
//                if (value != null)
//                {
//                    var enumerator = value.GetEnumerator();
//                    return enumerator.MoveNext();
//                }
//            }
//            return false;
//        }

//        // Вспомогательные методы для отрисовки линий (пока заглушки)
//        private bool IsLastChild(int idx) => false;
//        private object GetParentHierarchyInfo(int idx) => null;
//    }
//}


// [ПОЛНЫЙ]
using ForestM8rix.Columns;
using ForestM8rix.Rendering;
using ForestM8rix.StateManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ForestM8rix.Core;

public class ForestM8rixManager
{
    private readonly FrameworkElement _host;
    private readonly NodeRegistry _registry;
    private IEnumerable _rootNodes;

    // Добавляем это свойство для доступа из Рендереров
    public Control View => _host as Control;

    // Коллекция колонок, которой будет управлять Менеджер
    public List<ForestM8rixColumn> Columns { get; } = new List<ForestM8rixColumn>();

    // [TAG] Метод для удобного добавления колонки (опционально)
    public void AddColumn(ForestM8rixColumn column)
    {
        Columns.Add(column);
        _host.InvalidateVisual(); // Перерисовываем при изменении структуры
    }

    // [TAG] Физические параметры строки
    public double RowHeight { get; set; } = 25.0; // Базовая высота без масштаба

    // [TAG] Смещения (Offsets)
    // VerticalOffset храним в "количестве строк", чтобы проще считать индексы
    public double VerticalOffset { get; set; }
    public double HorizontalOffset { get; set; }

    // Метод для получения реальной высоты с учетом зума
    public double GetScaledRowHeight() => RowHeight * Scale;

    // [TAG] Инструкция по поиску детей, получаемая из View
    public Func<object, IEnumerable> ChildSelector { get; set; }

    // Проброс данных для отрисовки
    public object[] Nodes => _registry.Nodes;
    public int[] Levels => _registry.Levels;

    // Параметры отображения
    public double Scale { get; set; } = 1.0;
    public double IndentSize { get; set; } = 20.0;
    public double Dpi { get; private set; }
    public Typeface DefaultTypeface { get; private set; }

    public ForestM8rixManager(FrameworkElement host)
    {
        _host = host;
        _registry = new NodeRegistry(); // Скрытая инициализация
        ChildSelector = _ => null;      // По умолчанию детей нет
        UpdateVisualParams();
    }

    // Обновление метрик текста (DPI, шрифты из Хоста)
    public void UpdateVisualParams()
    {
        Dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;
        DefaultTypeface = new Typeface(
            (FontFamily)_host.GetValue(TextElement.FontFamilyProperty),
            (FontStyle)_host.GetValue(TextElement.FontStyleProperty),
            (FontWeight)_host.GetValue(TextElement.FontWeightProperty),
            (FontStretch)_host.GetValue(TextElement.FontStretchProperty));
    }

    // Главная точка входа данных
    public void SetItemsSource(IEnumerable items, Func<object, IEnumerable> selector)
    {
        _rootNodes = items;
        ChildSelector = selector;
        RefreshFlatList();
    }

    // [TAG] Пересборка топологии
    public void RefreshFlatList()
    {
        if (_rootNodes == null) return;

        // Используем NodeRegistry.Process, передавая обертку с проверкой Expanded
        _registry.Process(_rootNodes, GetExpandedChildrenInternal);

        // Просим View перерисоваться
        _host.InvalidateVisual();
    }

    // 1. [TAG] Получение уровня вложенности для отрисовки отступа
    public int GetLevel(object node)
    {
        if (Nodes == null || node == null) return 0;

        // Быстрый поиск индекса объекта в текущем плоском списке
        int idx = Array.IndexOf(Nodes, node);
        if (idx >= 0 && idx < Levels.Length)
        {
            return Levels[idx];
        }
        return 0;
    }

    private IEnumerable GetExpandedChildrenInternal(object node)
    {
        // Если узел развернут в глобальном реестре состояний - запрашиваем его детей через делегат
        if (ForestStateRegistry.IsExpanded(node))
        {
            return ChildSelector?.Invoke(node);
        }
        return null;
    }

    // [TAG] Обработка взаимодействия
    public void OnCanvasMouseDown(Point pos, bool ctrl, bool shift)
    {
        double rowH = ForestM8rixOptions.GetRowHeight(_host) * Scale;
        int rowIdx = (int)(pos.Y / rowH);

        if (rowIdx < 0 || rowIdx >= Nodes.Length) return;

        object clickedNode = Nodes[rowIdx];
        double indentX = Levels[rowIdx] * IndentSize * Scale;

        // Проверка попадания в область экспандера (треугольника)
        Rect expRect = new Rect(indentX, rowIdx * rowH, IndentSize * Scale, rowH);
        if (expRect.Contains(pos) && HasChildren(clickedNode))
        {
            // Используем Ваш метод Toggle
            ForestStateRegistry.Toggle(clickedNode);
            RefreshFlatList();
            return;
        }

        // Логика выделения
        if (!ctrl && !shift) ForestStateRegistry.ClearSelection();
        ForestStateRegistry.SetSelected(clickedNode, true);

        _host.InvalidateVisual();
    }

    // [TAG] Отрисовка строки
    public void RenderRow(DrawingContext dc, int idx, double y, double rowH, double viewW, List<ForestM8rixColumn> columns)
    {
        object node = Nodes[idx];
        int level = Levels[idx];
        bool isSelected = ForestStateRegistry.IsSelected(node);

        // 1. Фон строки
        Brush bg = isSelected ? ForestM8rixOptions.GetSelectionBackground(_host) : Brushes.Transparent;
        if (bg != Brushes.Transparent)
            dc.DrawRectangle(bg, null, new Rect(0, y, viewW, rowH));

        // 2. Отрисовка колонок (упрощенная версия для примера)
        double currentX = 0;
        foreach (var col in columns)
        {
            double colW = col.Width * Scale;
            // Тут вызывается конкретный Renderer для каждой ячейки
            var renderer = col.GetCellRenderer(this);
            renderer.Draw(dc, new Rect(currentX, y, colW, rowH), node, isSelected);
            currentX += colW;
        }
    }

    public bool HasChildren(object node)
    {
        var children = ChildSelector?.Invoke(node);
        if (children == null) return false;
        var en = children.GetEnumerator();
        return en.MoveNext();
    }

    // [ПОЛНЫЙ]
    public void RenderAll(DrawingContext dc, Size canvasSize)
    {
        // 1. Если данных нет или реестр пуст — выходим
        if (Nodes == null || Nodes.Length == 0) return;

        double sRowH = GetScaledRowHeight(); // Использует RowHeight * Scale

        // 2. [TAG] Простейшая виртуализация
        // Определяем индекс первой видимой строки на основе скролла
        // Мы берем чуть выше (Math.Max), чтобы не было "дырок" при быстром скролле
        int startIdx = (int)Math.Floor(VerticalOffset);
        if (startIdx < 0) startIdx = 0;

        // Определяем, сколько строк влезет в текущий размер холста
        int visibleCount = (int)Math.Ceiling(canvasSize.Height / sRowH) + 1;
        int endIdx = Math.Min(Nodes.Length, startIdx + visibleCount);

        // 3. Цикл отрисовки только тех строк, которые видит пользователь
        for (int i = startIdx; i < endIdx; i++)
        {
            // Рассчитываем позицию Y для конкретной строки
            // ВАЖНО: Мы рисуем на ForestCanvas, который находится внутри ScrollViewer,
            // поэтому координата Y должна быть абсолютной (i * высота), 
            // а скролл обработает сам ScrollViewer.
            double y = i * sRowH;

            // Вызываем детальную отрисовку строки (фон, колонки, текст)
            RenderRow(dc, i, y, sRowH, canvasSize.Width, Columns);
        }
    }

    // [ПОЛНЫЙ]
    public void SetSelection(object input)
    {
        // 1. Всегда начинаем с чистого листа (если не реализуем режим Add)
        ForestStateRegistry.ClearSelection();

        if (input == null)
        {
            _host.InvalidateVisual();
            return;
        }

        // 2. Проверяем: это коллекция или одиночный объект?
        // Мы исключаем string, так как это тоже IEnumerable, но обычно это данные (ID)
        if (input is IEnumerable collection && !(input is string))
        {
            foreach (var item in collection)
            {
                ForestStateRegistry.SetSelected(item, true);
            }
        }
        else
        {
            // Это одиночный объект
            ForestStateRegistry.SetSelected(input, true);
        }

        // 3. Обновляем экран
        _host.InvalidateVisual();
    }
}