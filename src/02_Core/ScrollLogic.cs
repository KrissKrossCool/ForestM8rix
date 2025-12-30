using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

//namespace ForestM8rix;

namespace ForestM8rix222
{
    public class ScrollData
    {
        public Vector Offset;
        public Size Extent;
        public Size Viewport;
    }
    public abstract class ScrollLogic : Control
{
    protected ScrollData _scrollData = new ScrollData();
    private ScrollBar _verticalScrollBar;
    private ScrollBar _horizontalScrollBar;

    public double ViewportHeight => _scrollData.Viewport.Height;

    public double VerticalOffset
    {
        get => _scrollData.Offset.Y;
        set => SetVerticalOffset(value);
    }

    public double HorizontalOffset
    {
        get => _scrollData.Offset.X;
        set => SetHorizontalOffset(value);
    }

    /// <summary>
    /// Прокручивает к указанному проценту от общей высоты.
    /// </summary>
    /// <param name="percent">Значение от 0.0 до 100.0</param>
    public void ScrollToPercent(double percent)
    {
        // Ограничиваем значение
        percent = Math.Max(0, Math.Min(100.0, percent));

        double maxOffset = Math.Max(0, _scrollData.Extent.Height - _scrollData.Viewport.Height);

        // Вычисляем целевой offset
        double targetOffset = maxOffset * (percent / 100.0);

        System.Diagnostics.Debug.WriteLine($"[DEBUG] Scrolling to {percent}%. Target Offset: {targetOffset}");

        SetVerticalOffset(targetOffset);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _verticalScrollBar = GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
        if (_verticalScrollBar != null)
            _verticalScrollBar.Scroll += (s, e) => SetVerticalOffset(e.NewValue);

        _horizontalScrollBar = GetTemplateChild("PART_HorizontalScrollBar") as ScrollBar;
        if (_horizontalScrollBar != null)
            _horizontalScrollBar.Scroll += (s, e) => SetHorizontalOffset(e.NewValue);

        UpdateLayout(); // Первый "пинок"
        UpdateScrollMetrics();
    }

    public void SetVerticalOffset(double offset)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] SetVerticalOffset CALLED with incoming offset: {offset:F2}");

        // 1. Рассчитываем максимальное возможное смещение
        double maxOffset = Math.Max(0, _scrollData.Extent.Height - _scrollData.Viewport.Height);

        // 2. Ограничиваем запрошенное значение в допустимом диапазоне [0, maxOffset]
        double newOffset = Math.Max(0, Math.Min(offset, maxOffset));

        System.Diagnostics.Debug.WriteLine($"    -> Clamped new offset: {newOffset:F2}");

        // 3. Применяем значение, только если оно действительно изменилось
        if (Math.Abs(_scrollData.Offset.Y - newOffset) > 0.001)
        {
            _scrollData.Offset.Y = newOffset;
            System.Diagnostics.Debug.WriteLine($"    -> SUCCESS: _scrollData.Offset.Y is now {_scrollData.Offset.Y:F2}");

            // 4. Синхронизируем UI
            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.Value = newOffset;
            }

            // 5. Принудительно обновляем макет, чтобы UI (включая ScrollBar) немедленно отреагировал
            //UpdateLayout();
            InvalidateVisual();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"    -> SKIPPED: Value did not change enough. Current: {_scrollData.Offset.Y:F2}");
        }
    }


    public void SetHorizontalOffset(double offset)
    {
        double max = Math.Max(0, _scrollData.Extent.Width - _scrollData.Viewport.Width);
        offset = Math.Max(0, Math.Min(offset, max));

        if (Math.Abs(_scrollData.Offset.X - offset) < 0.001) return;

        _scrollData.Offset.X = offset;
        if (_horizontalScrollBar != null) _horizontalScrollBar.Value = offset;

        UpdateLayout();
        InvalidateVisual();
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            double delta = (e.Delta / 120.0) * 30;
            SetHorizontalOffset(HorizontalOffset - delta);
        }
        else
        {
            double delta = (e.Delta / 120.0) * 3;
            SetVerticalOffset(VerticalOffset - delta);
        }
        e.Handled = true;
        base.OnPreviewMouseWheel(e);
    }

    protected void UpdateScrollMetrics()
    {
        if (_verticalScrollBar != null)
        {
            double maxV = Math.Max(0, _scrollData.Extent.Height - _scrollData.Viewport.Height);
            if (Math.Abs(_verticalScrollBar.Maximum - maxV) > 0.01) _verticalScrollBar.Maximum = maxV;
            if (Math.Abs(_verticalScrollBar.ViewportSize - _scrollData.Viewport.Height) > 0.01) _verticalScrollBar.ViewportSize = _scrollData.Viewport.Height;

            var newVisibility = maxV > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (_verticalScrollBar.Visibility != newVisibility) _verticalScrollBar.Visibility = newVisibility;
        }

        if (_horizontalScrollBar != null)
        {
            double maxH = Math.Max(0, _scrollData.Extent.Width - _scrollData.Viewport.Width);
            if (Math.Abs(_horizontalScrollBar.Maximum - maxH) > 0.01) _horizontalScrollBar.Maximum = maxH;
            if (Math.Abs(_horizontalScrollBar.ViewportSize - _scrollData.Viewport.Width) > 0.01) _horizontalScrollBar.ViewportSize = _scrollData.Viewport.Width;

            var newVisibility = maxH > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (_horizontalScrollBar.Visibility != newVisibility) _horizontalScrollBar.Visibility = newVisibility;
        }
    }
}

}

namespace ForestM8rix
{
    public class ScrollData
    {
        public ScrollViewer ScrollOwner;
        public Vector Offset;
        public Size Extent;
        public Size Viewport;
    }

    public abstract class ScrollLogic : Control, IScrollInfo
    {
        protected ScrollData _scrollData = new ScrollData();

        public bool CanVerticallyScroll { get; set; } = true;
        public bool CanHorizontallyScroll { get; set; } = true;

        public double ExtentHeight => _scrollData.Extent.Height;
        public double ViewportHeight => _scrollData.Viewport.Height;
        public double VerticalOffset => _scrollData.Offset.Y;

        public double ExtentWidth => _scrollData.Extent.Width;
        public double ViewportWidth => _scrollData.Viewport.Width;
        public double HorizontalOffset => _scrollData.Offset.X;

        public ScrollViewer ScrollOwner
        {
            get => _scrollData.ScrollOwner;
            set => _scrollData.ScrollOwner = value;
        }

        public void SetVerticalOffset(double offset)
        {
            double maxOffset = Math.Max(0, ExtentHeight - ViewportHeight);
            offset = Math.Max(0, Math.Min(offset, maxOffset));

            if (Math.Abs(_scrollData.Offset.Y - offset) > 0.001)
            {
                _scrollData.Offset.Y = offset;
                ScrollOwner?.InvalidateScrollInfo();
                InvalidateVisual();
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            double maxOffset = Math.Max(0, ExtentWidth - ViewportWidth);
            offset = Math.Max(0, Math.Min(offset, maxOffset));

            if (Math.Abs(_scrollData.Offset.X - offset) > 0.001)
            {
                _scrollData.Offset.X = offset;
                ScrollOwner?.InvalidateScrollInfo();
                InvalidateVisual();
            }
        }

        public void LineUp() => SetVerticalOffset(VerticalOffset - 1);
        public void LineDown() => SetVerticalOffset(VerticalOffset + 1);
        public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
        public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
        public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - SystemParameters.WheelScrollLines);
        public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + SystemParameters.WheelScrollLines);

        public void LineLeft() => SetHorizontalOffset(HorizontalOffset - 10);
        public void LineRight() => SetHorizontalOffset(HorizontalOffset + 10);
        public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - 30);
        public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + 30);

        public Rect MakeVisible(Visual visual, Rect rectangle) => Rect.Empty;
    }
}

