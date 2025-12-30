using ForestM8rix.StateManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ForestM8rix;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
public class CoreInternalAttribute : Attribute { }

public class ForestM8rixManager
{
    private readonly NodeRegistry _registry = new();
    private readonly UIElement _host;
    private IEnumerable _source;
    private Func<object, IEnumerable> _childSelector;
    private int _updateCounter = 0;

    // --- STATE MANAGEMENT ---
    public int HoveredRowIndex { get; private set; } = -1;
    private object _lastSelectedNode = null;
    private int _anchorIndex = -1; // [NEW] Якорь для Shift-выделения
    public Rect SelectionRect { get; private set; } = Rect.Empty;
    public bool IsRubberBandEnabled { get; set; } = true;

    // --- SCROLL & LAYOUT ---
    public List<ForestColumn> Columns { get; } = new();
    public double RowHeight { get; set; } = 24.0;
    public double VerticalOffset { get; set; } = 0;
    public double HorizontalOffset { get; set; } = 0;
    public double TotalWidth => Columns.Sum(c => c.Width);

    public int Count => _registry.Count;
    public object[] Nodes => _registry.Nodes;
    public int[] Levels => _registry.Levels;

    public event EventHandler<CommandExecutedEventArgs> CommandExecuted;
    public event Action<int> RequestScrollIntoView; // [NEW]

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
        if (Nodes == null || Nodes.Length == 0)
        {
            DrawErrorText(dc, "НЕТ ДАННЫХ (Nodes is null)", Brushes.Yellow, 50);
            return;
        }

        if (Columns.Count == 0)
        {
            DrawErrorText(dc, "КОЛОНКИ НЕ НАСТРОЕНЫ", Brushes.Orange, 80);
            return;
        }

        double headerHeight = 30;
        double dpi = VisualTreeHelper.GetDpi(_host).PixelsPerDip;
        Typeface typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        Pen gridPen = new Pen(Brushes.LightGray, 0.5);

        // === 1. ОТРИСОВКА ШАПКИ ===
        dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, renderSize.Width, headerHeight));
        double colX = -HorizontalOffset;

        dc.PushClip(new RectangleGeometry(new Rect(0, 0, renderSize.Width, headerHeight)));
        foreach (var col in Columns)
        {
            if (colX + col.Width > 0 && colX < renderSize.Width)
            {
                var headFt = new FormattedText(col.Header, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface(typeface.FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 12, Brushes.DimGray, dpi);

                dc.DrawText(headFt, new Point(colX + 5, (headerHeight - headFt.Height) / 2));
                dc.DrawLine(gridPen, new Point(colX + col.Width, 0), new Point(colX + col.Width, headerHeight));
            }
            colX += col.Width;
        }
        dc.Pop();
        dc.DrawLine(gridPen, new Point(0, headerHeight), new Point(renderSize.Width, headerHeight));

        // === 2. ОТРИСОВКА СТРОК ===
        double currentY = headerHeight - (VerticalOffset * RowHeight);

        dc.PushClip(new RectangleGeometry(new Rect(0, headerHeight, renderSize.Width, renderSize.Height - headerHeight)));

        for (int i = 0; i < Nodes.Length; i++)
        {
            if (currentY + RowHeight < headerHeight) { currentY += RowHeight; continue; }
            if (currentY > renderSize.Height) break;

            object node = Nodes[i];

            bool isSelected = ForestStateRegistry.IsSelected(node);

            if (isSelected) dc.DrawRectangle(Brushes.LightSkyBlue, null, new Rect(0, currentY, renderSize.Width, RowHeight));
            else if (i == HoveredRowIndex) dc.DrawRectangle(Brushes.AliceBlue, null, new Rect(0, currentY, renderSize.Width, RowHeight));

            int level = Levels[i];
            double cellX = -HorizontalOffset;

            dc.DrawLine(gridPen, new Point(0, currentY + RowHeight), new Point(renderSize.Width, currentY + RowHeight));

            for (int c = 0; c < Columns.Count; c++)
            {
                var col = Columns[c];

                if (cellX + col.Width > 0 && cellX < renderSize.Width)
                {
                    double textX = cellX + 5;
                    if (c == 0)
                    {
                        double indent = level * 20.0;
                        if (HasChildren(node))
                        {
                            bool isExp = ForestStateRegistry.IsExpanded(node);
                            DrawExpander(dc, new Point(cellX + indent + 10, currentY + RowHeight / 2), isExp);
                        }
                        textX = cellX + indent + 25;
                    }

                    string text = col.CellTextSelector?.Invoke(node) ?? "";
                    var ft = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, dpi);

                    dc.PushClip(new RectangleGeometry(new Rect(cellX, currentY, col.Width, RowHeight)));
                    dc.DrawText(ft, new Point(textX, currentY + (RowHeight - ft.Height) / 2));
                    dc.Pop();
                }
                cellX += col.Width;
            }
            currentY += RowHeight;
        }
        dc.Pop();

        if (IsRubberBandEnabled && !SelectionRect.IsEmpty)
        {
            var brush = new SolidColorBrush(Color.FromArgb(76, 51, 153, 255));
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
            if (isExpanded)
            {
                context.BeginFigure(new Point(center.X - 4, center.Y - 2), true, true);
                context.LineTo(new Point(center.X + 4, center.Y - 2), true, false);
                context.LineTo(new Point(center.X, center.Y + 3), true, false);
            }
            else
            {
                context.BeginFigure(new Point(center.X - 2, center.Y - 4), true, true);
                context.LineTo(new Point(center.X - 2, center.Y + 4), true, false);
                context.LineTo(new Point(center.X + 3, center.Y), true, false);
            }
        }
        dc.DrawGeometry(Brushes.Gray, null, geometry);
    }

    #endregion

    #region Input Handling (Mouse & Keyboard)

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
        if (!IsRubberBandEnabled) return;

        double x = Math.Min(start.X, end.X);
        double y = Math.Min(start.Y, end.Y);
        double w = Math.Abs(end.X - start.X);
        double h = Math.Abs(end.Y - start.Y);
        SelectionRect = new Rect(x, y, w, h);

        double headerHeight = 30;
        if (y + h < headerHeight) return;

        double dataTop = Math.Max(y, headerHeight) - headerHeight;
        double dataBottom = (y + h) - headerHeight;

        int idxStart = (int)(dataTop / RowHeight) + (int)VerticalOffset;
        int idxEnd = (int)(dataBottom / RowHeight) + (int)VerticalOffset;

        if (Nodes == null) return;
        idxStart = Math.Max(0, idxStart);
        idxEnd = Math.Min(Nodes.Length - 1, idxEnd);

        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            ForestStateRegistry.ClearSelection();
        }

        for (int i = idxStart; i <= idxEnd; i++)
        {
            ForestStateRegistry.SetSelected(Nodes[i], true);
        }

        _host.InvalidateVisual();
    }

    public void ClearSelectionRect()
    {
        SelectionRect = Rect.Empty;
        _host.InvalidateVisual();
    }

    public void HandleClick(Point point)
    {
        double headerHeight = 30;
        if (point.Y < headerHeight) return;

        int rowIndex = (int)((point.Y - headerHeight) / RowHeight) + (int)VerticalOffset;

        if (Nodes != null && rowIndex >= 0 && rowIndex < Nodes.Length)
        {
            object node = Nodes[rowIndex];
            int level = Levels[rowIndex];

            double indent = level * 20.0;
            // Учет горизонтального скролла для экспандера
            double expanderX = indent + 10 - HorizontalOffset;

            // Зона клика по треугольнику (чуть шире)
            if (point.X >= expanderX - 5 && point.X <= expanderX + 20 && HasChildren(node))
            {
                ForestStateRegistry.Toggle(node);
                Refresh();
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    bool currentState = ForestStateRegistry.IsSelected(node);
                    ForestStateRegistry.SetSelected(node, !currentState);
                    _lastSelectedNode = node;
                    _anchorIndex = rowIndex;
                }
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && _lastSelectedNode != null)
                {
                    // Логика Shift+Click: от якоря до клика
                    if (_anchorIndex == -1)
                        _anchorIndex = Array.IndexOf(Nodes, _lastSelectedNode); // fallback

                    ForestStateRegistry.ClearSelection();

                    int start = Math.Min(_anchorIndex, rowIndex);
                    int end = Math.Max(_anchorIndex, rowIndex);

                    for (int i = start; i <= end; i++) ForestStateRegistry.SetSelected(Nodes[i], true);

                    _lastSelectedNode = node;
                }
                else
                {
                    ForestStateRegistry.ClearSelection();
                    ForestStateRegistry.SetSelected(node, true);
                    _lastSelectedNode = node;
                    _anchorIndex = rowIndex;
                }
                _host.InvalidateVisual();
            }
        }
    }

    public void HandleKeyDown(Key key)
    {
        if (Nodes == null || Nodes.Length == 0) return;

        int currentIndex = -1;
        if (_lastSelectedNode != null)
        {
            currentIndex = Array.IndexOf(Nodes, _lastSelectedNode);
        }

        if (currentIndex == -1)
        {
            SelectRow(0, false);
            return;
        }

        bool isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        switch (key)
        {
            case Key.Up:
                if (currentIndex > 0)
                    SelectRow(currentIndex - 1, isShift);
                break;

            case Key.Down:
                if (currentIndex < Nodes.Length - 1)
                    SelectRow(currentIndex + 1, isShift);
                break;

            case Key.Right:
                HandleRightArrow(currentIndex);
                break;

            case Key.Left:
                HandleLeftArrow(currentIndex);
                break;
        }
    }

    private void SelectRow(int targetIndex, bool isShift)
    {
        var targetNode = Nodes[targetIndex];

        if (isShift)
        {
            if (_anchorIndex == -1) _anchorIndex = targetIndex;

            ForestStateRegistry.ClearSelection();

            int start = Math.Min(_anchorIndex, targetIndex);
            int end = Math.Max(_anchorIndex, targetIndex);

            for (int i = start; i <= end; i++)
            {
                ForestStateRegistry.SetSelected(Nodes[i], true);
            }
        }
        else
        {
            ForestStateRegistry.ClearSelection();
            ForestStateRegistry.SetSelected(targetNode, true);
            _anchorIndex = targetIndex;
        }

        _lastSelectedNode = targetNode;
        _host.InvalidateVisual();
        RequestScrollIntoView?.Invoke(targetIndex);
    }

    private void HandleRightArrow(int index)
    {
        var node = Nodes[index];
        if (HasChildren(node))
        {
            if (!ForestStateRegistry.IsExpanded(node))
            {
                ForestStateRegistry.SetExpanded(node, true);
                Refresh();
            }
            else
            {
                if (index < Nodes.Length - 1) SelectRow(index + 1, false);
            }
        }
    }

    private void HandleLeftArrow(int index)
    {
        var node = Nodes[index];
        if (HasChildren(node) && ForestStateRegistry.IsExpanded(node))
        {
            ForestStateRegistry.SetExpanded(node, false);
            Refresh();
            RequestScrollIntoView?.Invoke(index);
        }
        else
        {
            int currentLevel = Levels[index];
            if (currentLevel > 0)
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (Levels[i] < currentLevel)
                    {
                        SelectRow(i, false);
                        return;
                    }
                }
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
