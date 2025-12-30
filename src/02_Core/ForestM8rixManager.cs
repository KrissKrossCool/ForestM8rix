//using ForestM8rix.StateManagement;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Threading;
//using System.Windows;
//using System.Windows.Media;

//namespace ForestM8rix;

//[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
//public class CoreInternalAttribute : Attribute { }

//public class ForestM8rixManager
//{
//    private readonly NodeRegistry _registry = new();
//    private readonly UIElement _host;
//    private IEnumerable _source;
//    private Func<object, IEnumerable> _childSelector;
//    private int _updateCounter = 0;

//    // Новое: Список колонок
//    public List<ForestColumn> Columns { get; } = new();

//    public double RowHeight { get; set; } = 24.0;
//    public double VerticalOffset { get; set; } = 0;

//    public int Count => _registry.Count;
//    public object[] Nodes => _registry.Nodes;
//    public int[] Levels => _registry.Levels;

//    public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

//    public ForestM8rixManager(UIElement host)
//    {
//        _host = host;

//        // [СУТЬ] Если этого окна нет при запуске — мы правим "труп" кода
//        //System.Windows.MessageBox.Show("MANAGER CONSTRUCTOR CALLED");
//    }

//    #region Управление данными и Обновление

//    public void SetSource(IEnumerable source, Func<object, IEnumerable> childSelector)
//    {
//        _source = source;
//        _childSelector = childSelector;
//        Refresh();
//    }

//    public IDisposable DeferRefresh() => new ForestUpdateContext(this);
//    internal void BeginUpdate() => Interlocked.Increment(ref _updateCounter);
//    internal void EndUpdate(string cmdName = "Generic", object result = null)
//    {
//        if (Interlocked.Decrement(ref _updateCounter) == 0)
//        {
//            Refresh();
//            CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(cmdName, result));
//        }
//    }

//    public void Refresh()
//    {
//        if (_source == null) return;
//        _registry.Process(_source, node => ForestStateRegistry.IsExpanded(node) ? _childSelector?.Invoke(node) : null);
//        _host.InvalidateVisual();
//    }

//    #endregion

//    #region Отрисовка (Табличный режим)

//    [CoreInternal]
//    public void Render(DrawingContext dc, Size renderSize)
//    {
//        // [ДАТЧИК] Если данных нет вообще
//        if (Nodes == null || Nodes.Length == 0)
//        {
//            var ftErr = new FormattedText("НЕТ ДАННЫХ (Nodes is null)", System.Globalization.CultureInfo.InvariantCulture,
//                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 20, Brushes.Yellow, VisualTreeHelper.GetDpi(_host).PixelsPerDip);
//            dc.DrawText(ftErr, new Point(10, 50));
//            return;
//        }

//        // [ДАТЧИК] Если колонки не настроены
//        if (Columns.Count == 0)
//        {
//            var ftErr = new FormattedText("КОЛОНКИ НЕ НАСТРОЕНЫ (Columns.Count == 0)", System.Globalization.CultureInfo.InvariantCulture,
//                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 20, Brushes.Orange, VisualTreeHelper.GetDpi(_host).PixelsPerDip);
//            dc.DrawText(ftErr, new Point(10, 80));
//            // Для теста рисуем хотя бы дефолтный текст, раз колонок нет
//            for (int i = 0; i < Nodes.Length; i++)
//                dc.DrawText(new FormattedText(Nodes[i].ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.White, 1.0), new Point(10, 110 + i * 20));
//            return;
//        }

//        double headerHeight = 30;
//        double dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;
//        Typeface typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
//        Pen gridPen = new Pen(Brushes.LightGray, 0.5);

//        // 1. ОТРИСОВКА ШАПКИ
//        dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, renderSize.Width, headerHeight));
//        double colX = 0;
//        foreach (var col in Columns)
//        {
//            var headFt = new FormattedText(col.Header, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
//                new Typeface(typeface.FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 12, Brushes.DimGray, dpi);
//            dc.DrawText(headFt, new Point(colX + 5, (headerHeight - headFt.Height) / 2));
//            colX += col.Width;
//            dc.DrawLine(gridPen, new Point(colX, 0), new Point(colX, renderSize.Height));
//        }
//        dc.DrawLine(gridPen, new Point(0, headerHeight), new Point(renderSize.Width, headerHeight));

//        // 2. ОТРИСОВКА СТРОК
//        double currentY = headerHeight - (VerticalOffset * RowHeight);

//        for (int i = 0; i < Nodes.Length; i++)
//        {
//            if (currentY + RowHeight < headerHeight) { currentY += RowHeight; continue; }
//            if (currentY > renderSize.Height) break;

//            object node = Nodes[i];
//            int level = Levels[i];
//            double cellX = 0;

//            dc.DrawLine(gridPen, new Point(0, currentY + RowHeight), new Point(renderSize.Width, currentY + RowHeight));

//            for (int c = 0; c < Columns.Count; c++)
//            {
//                var col = Columns[c];
//                double textX = cellX + 5;

//                if (c == 0) // Первая колонка (Дерево)
//                {
//                    double indent = level * 20.0;
//                    if (HasChildren(node))
//                    {
//                        bool isExp = StateManagement.ForestStateRegistry.IsExpanded(node);
//                        DrawExpander(dc, new Point(cellX + indent + 10, currentY + RowHeight / 2), isExp);
//                    }
//                    textX = cellX + indent + 25;
//                }

//                string text = col.CellTextSelector?.Invoke(node) ?? "null";
//                var ft = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, dpi);

//                dc.PushClip(new RectangleGeometry(new Rect(cellX, currentY, col.Width, RowHeight)));
//                dc.DrawText(ft, new Point(textX, currentY + (RowHeight - ft.Height) / 2));
//                dc.Pop();

//                cellX += col.Width;
//            }
//            currentY += RowHeight;
//        }
//    }

//    private void DrawExpander(DrawingContext dc, Point center, bool isExpanded)
//    {
//        // СУТЬ: Простая отрисовка треугольника геометрии
//        var geometry = new StreamGeometry();
//        using (var context = geometry.Open())
//        {
//            if (isExpanded)
//            { // Вниз
//                context.BeginFigure(new Point(center.X - 4, center.Y - 2), true, true);
//                context.LineTo(new Point(center.X + 4, center.Y - 2), true, false);
//                context.LineTo(new Point(center.X, center.Y + 3), true, false);
//            }
//            else
//            { // Вправо
//                context.BeginFigure(new Point(center.X - 2, center.Y - 4), true, true);
//                context.LineTo(new Point(center.X - 2, center.Y + 4), true, false);
//                context.LineTo(new Point(center.X + 3, center.Y), true, false);
//            }
//        }
//        dc.DrawGeometry(Brushes.Gray, null, geometry);
//    }

//    #endregion

//    public void HandleClick(Point point)
//    {
//        double headerHeight = 30; // Должно совпадать с высотой в Render

//        // 1. СЕКЬЮРИТИ: Клик в шапку не должен раскрывать строки
//        if (point.Y < headerHeight) return;

//        // 2. РАСЧЕТ: Переводим пиксели в индекс узла
//        // Формула: (КликY - Шапка) / ВысотаСтроки + СмещениеСкролла
//        int rowIndex = (int)((point.Y - headerHeight) / RowHeight) + (int)VerticalOffset;

//        // ОТЛАДКА: Посмотри в Output, совпадает ли индекс с визуальной строкой
//        System.Diagnostics.Debug.WriteLine($"MANAGER_CLICK: Row={rowIndex}, TotalNodes={Nodes?.Length}");

//        if (Nodes != null && rowIndex >= 0 && rowIndex < Nodes.Length)
//        {
//            object node = Nodes[rowIndex];

//            // 3. ДЕЙСТВИЕ: Переключаем состояние (Open/Close)
//            StateManagement.ForestStateRegistry.Toggle(node);

//            // 4. ОБНОВЛЕНИЕ: Перестраиваем плоский список Nodes
//            Refresh();
//        }
//    }

//    public bool HasChildren(object node)
//    {
//        if (node == null || _childSelector == null) return false;

//        // Получаем результат через селектор, заданный в SetSource
//        var children = _childSelector(node);
//        if (children == null) return false;

//        // Оптимизация: если это коллекция, проверяем Count
//        if (children is ICollection coll) return coll.Count > 0;

//        // Универсальная проверка для любого IEnumerable
//        return children.Cast<object>().Any();
//    }

//    public TResult Run<TResult>(IForestCommand<TResult> command)
//    {
//        using (DeferRefresh()) return command.Execute(this);
//    }
//}


using ForestM8rix.StateManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input; // Важно для Keyboard.Modifiers
using System.Windows.Media;

namespace ForestM8rix;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
public class CoreInternalAttribute : Attribute { }

public class ForestM8rixManager
{

    // Настройка управление вкл/выкл - возможности выбора рамкой
    public bool IsRubberBandEnabled { get; set; } = true;

    private readonly NodeRegistry _registry = new();
    private readonly UIElement _host;
    private IEnumerable _source;
    private Func<object, IEnumerable> _childSelector;
    private int _updateCounter = 0;

    // --- STATE MANAGEMENT ---
    public int HoveredRowIndex { get; private set; } = -1;
    private object _lastSelectedNode = null;
    public Rect SelectionRect { get; private set; } = Rect.Empty;
    // ------------------------

    // Новое: Список колонок
    public List<ForestColumn> Columns { get; } = new();

    public double RowHeight { get; set; } = 24.0;
    public double VerticalOffset { get; set; } = 0;

    public int Count => _registry.Count;
    public object[] Nodes => _registry.Nodes;
    public int[] Levels => _registry.Levels;

    public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

    public ForestM8rixManager(UIElement host)
    {
        _host = host;
    }

    #region Управление данными и Обновление

    public void SetSource(IEnumerable source, Func<object, IEnumerable> childSelector)
    {
        _source = source;
        _childSelector = childSelector;
        Refresh();
    }

    public IDisposable DeferRefresh() => new ForestUpdateContext(this);

    internal void BeginUpdate() => Interlocked.Increment(ref _updateCounter);

    internal void EndUpdate(string cmdName = "Generic", object result = null)
    {
        if (Interlocked.Decrement(ref _updateCounter) == 0)
        {
            Refresh();
            CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(cmdName, result));
        }
    }

    public void Refresh()
    {
        if (_source == null) return;
        _registry.Process(_source, node => ForestStateRegistry.IsExpanded(node) ? _childSelector?.Invoke(node) : null);
        _host.InvalidateVisual();
    }

    #endregion

    #region Отрисовка (Табличный режим)

    [CoreInternal]
    public void Render(DrawingContext dc, Size renderSize)
    {
        // [ДАТЧИК] Если данных нет вообще
        if (Nodes == null || Nodes.Length == 0)
        {
            DrawErrorText(dc, "НЕТ ДАННЫХ (Nodes is null)", Brushes.Yellow, 50);
            return;
        }

        // [ДАТЧИК] Если колонки не настроены
        if (Columns.Count == 0)
        {
            DrawErrorText(dc, "КОЛОНКИ НЕ НАСТРОЕНЫ", Brushes.Orange, 80);
            return;
        }

        double headerHeight = 30;
        double dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;
        Typeface typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        Pen gridPen = new Pen(Brushes.LightGray, 0.5);

        // 1. ОТРИСОВКА ШАПКИ
        dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, renderSize.Width, headerHeight));

        double colX = 0;
        foreach (var col in Columns)
        {
            var headFt = new FormattedText(col.Header, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(typeface.FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 12, Brushes.DimGray, dpi);

            dc.DrawText(headFt, new Point(colX + 5, (headerHeight - headFt.Height) / 2));
            colX += col.Width;
            dc.DrawLine(gridPen, new Point(colX, 0), new Point(colX, renderSize.Height));
        }
        dc.DrawLine(gridPen, new Point(0, headerHeight), new Point(renderSize.Width, headerHeight));

        // 2. ОТРИСОВКА СТРОК
        double currentY = headerHeight - (VerticalOffset * RowHeight);

        for (int i = 0; i < Nodes.Length; i++)
        {
            if (currentY + RowHeight < headerHeight) { currentY += RowHeight; continue; }
            if (currentY > renderSize.Height) break;

            object node = Nodes[i];

            // --- DRAW BACKGROUNDS (Selection & Hover) ---
            bool isSelected = ForestStateRegistry.IsSelected(node);

            if (isSelected)
            {
                dc.DrawRectangle(Brushes.LightSkyBlue, null, new Rect(0, currentY, renderSize.Width, RowHeight));
            }
            else if (i == HoveredRowIndex)
            {
                dc.DrawRectangle(Brushes.AliceBlue, null, new Rect(0, currentY, renderSize.Width, RowHeight));
            }
            // --------------------------------------------

            int level = Levels[i];
            double cellX = 0;

            dc.DrawLine(gridPen, new Point(0, currentY + RowHeight), new Point(renderSize.Width, currentY + RowHeight));

            for (int c = 0; c < Columns.Count; c++)
            {
                var col = Columns[c];
                double textX = cellX + 5;

                if (c == 0) // Первая колонка (Дерево)
                {
                    double indent = level * 20.0;
                    if (HasChildren(node))
                    {
                        bool isExp = ForestStateRegistry.IsExpanded(node);
                        DrawExpander(dc, new Point(cellX + indent + 10, currentY + RowHeight / 2), isExp);
                    }
                    textX = cellX + indent + 25;
                }

                string text = col.CellTextSelector?.Invoke(node) ?? "null";
                var ft = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, dpi);

                // Clipping для текста, чтобы не вылезал за ячейку
                dc.PushClip(new RectangleGeometry(new Rect(cellX, currentY, col.Width, RowHeight)));
                dc.DrawText(ft, new Point(textX, currentY + (RowHeight - ft.Height) / 2));
                dc.Pop();

                cellX += col.Width;
            }

            currentY += RowHeight;
        }

        // --- DRAW RUBBER BAND (Selection Rect) ---
        if (!SelectionRect.IsEmpty)
        {
            var brush = new SolidColorBrush(Color.FromArgb(76, 51, 153, 255)); // Semi-transparent Blue
            var border = new Pen(Brushes.RoyalBlue, 1);
            dc.DrawRectangle(brush, border, SelectionRect);
        }
    }

    private void DrawErrorText(DrawingContext dc, string text, Brush color, double y)
    {
        var ft = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture,
               FlowDirection.LeftToRight, new Typeface("Segoe UI"), 20, color, VisualTreeHelper.GetDpi(_host).PixelsPerDip);
        dc.DrawText(ft, new Point(10, y));
    }

    private void DrawExpander(DrawingContext dc, Point center, bool isExpanded)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            if (isExpanded) // Вниз
            {
                context.BeginFigure(new Point(center.X - 4, center.Y - 2), true, true);
                context.LineTo(new Point(center.X + 4, center.Y - 2), true, false);
                context.LineTo(new Point(center.X, center.Y + 3), true, false);
            }
            else // Вправо
            {
                context.BeginFigure(new Point(center.X - 2, center.Y - 4), true, true);
                context.LineTo(new Point(center.X - 2, center.Y + 4), true, false);
                context.LineTo(new Point(center.X + 3, center.Y), true, false);
            }
        }
        dc.DrawGeometry(Brushes.Gray, null, geometry);
    }

    #endregion

    #region Input Handling

    public void HandleMouseMove(Point point)
    {
        double headerHeight = 30;
        int rowIndex = -1;

        if (point.Y >= headerHeight)
        {
            rowIndex = (int)((point.Y - headerHeight) / RowHeight) + (int)VerticalOffset;
            if (rowIndex < 0 || Nodes == null || rowIndex >= Nodes.Length) rowIndex = -1;
        }

        if (HoveredRowIndex != rowIndex)
        {
            HoveredRowIndex = rowIndex;
            _host.InvalidateVisual();
        }
    }

    public void HandleMouseLeave()
    {
        if (HoveredRowIndex != -1)
        {
            HoveredRowIndex = -1;
            _host.InvalidateVisual();
        }
    }



    public void UpdateSelectionRect(Point start, Point end)
    {
        // Защита: Если функционал выключен, ничего не делаем
        if (!IsRubberBandEnabled) return;

        double x = Math.Min(start.X, end.X);
        double y = Math.Min(start.Y, end.Y);
        double w = Math.Abs(end.X - start.X);
        double h = Math.Abs(end.Y - start.Y);
        SelectionRect = new Rect(x, y, w, h);

        double headerHeight = 30;

        // Защита от выхода за пределы данных
        if (y + h < headerHeight) return;

        double dataTop = Math.Max(y, headerHeight) - headerHeight;
        double dataBottom = (y + h) - headerHeight;

        int idxStart = (int)(dataTop / RowHeight) + (int)VerticalOffset;
        int idxEnd = (int)(dataBottom / RowHeight) + (int)VerticalOffset;

        if (Nodes == null) return;
        idxStart = Math.Max(0, idxStart);
        idxEnd = Math.Min(Nodes.Length - 1, idxEnd);

        // Если не Ctrl — сбрасываем старое
        if (!System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
        {
            StateManagement.ForestStateRegistry.ClearSelection();
        }

        for (int i = idxStart; i <= idxEnd; i++)
        {
            StateManagement.ForestStateRegistry.SetSelected(Nodes[i], true);
        }

        _host.InvalidateVisual();
    }


    public void ClearSelectionRect()
    {
        // СУТЬ: Принудительно затираем рамку и перерисовываем, 
        // даже если система думает, что она пустая (защита от артефактов)
        SelectionRect = Rect.Empty;
        _host.InvalidateVisual();
    }




    public void HandleClick(Point point)
    {
        double headerHeight = 30;
        // Output debug logic removed for clean production code, add if needed
        // [LOG] Вход в метод
        System.Diagnostics.Debug.WriteLine($"[DEBUG] MANAGER: HandleClick entered. Point={point.Y:F1}");

        if (point.Y < headerHeight) return;

        int rowIndex = (int)((point.Y - headerHeight) / RowHeight) + (int)VerticalOffset;

        System.Diagnostics.Debug.WriteLine($"[DEBUG] MANAGER: Calculated RowIndex={rowIndex}. TotalNodes={Nodes?.Length}");

        if (Nodes != null && rowIndex >= 0 && rowIndex < Nodes.Length)
        {
            object node = Nodes[rowIndex];
            int level = Levels[rowIndex];

            double indent = level * 20.0;
            // Зона клика по треугольнику
            if (point.X >= indent && point.X <= indent + 30 && HasChildren(node))
            {
                ForestStateRegistry.Toggle(node);
                Refresh();
            }
            else
            {
                // --- MULTI-SELECTION LOGIC ---
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    bool currentState = ForestStateRegistry.IsSelected(node);
                    ForestStateRegistry.SetSelected(node, !currentState);
                    _lastSelectedNode = node;
                }
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && _lastSelectedNode != null)
                {
                    int lastIdx = Array.IndexOf(Nodes, _lastSelectedNode);
                    if (lastIdx != -1)
                    {
                        int start = Math.Min(lastIdx, rowIndex);
                        int end = Math.Max(lastIdx, rowIndex);
                        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                            ForestStateRegistry.ClearSelection();
                        for (int i = start; i <= end; i++) ForestStateRegistry.SetSelected(Nodes[i], true);
                    }
                    else
                    {
                        ForestStateRegistry.ClearSelection();
                        ForestStateRegistry.SetSelected(node, true);
                        _lastSelectedNode = node;
                    }
                }
                else
                {
                    ForestStateRegistry.ClearSelection();
                    ForestStateRegistry.SetSelected(node, true);
                    _lastSelectedNode = node;
                }
                _host.InvalidateVisual();
            }
        }
    }

    #endregion

    public bool HasChildren(object node)
    {
        if (node == null || _childSelector == null) return false;
        var children = _childSelector(node);
        if (children == null) return false;
        if (children is ICollection coll) return coll.Count > 0;
        return children.Cast<object>().Any();
    }

    public TResult Run<TResult>(IForestCommand<TResult> command)
    {
        using (DeferRefresh()) return command.Execute(this);
    }
}
