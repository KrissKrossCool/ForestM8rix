using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ForestM8rix
{

    //public class ScrollData
    //{
    //    // Viewport - сколько строк влезает в окно
    //    public Size Viewport;
    //    // Extent - общее кол-во строк в BitArray
    //    public Size Extent;
    //    // Offset - текущий индекс верхней видимой строки
    //    public Vector Offset;

    //    public ScrollViewer ScrollOwner { get; set; }
    //}

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _verticalScrollBar = GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.Scroll += (s, e) => SetVerticalOffset(e.NewValue);
            }

            _horizontalScrollBar = GetTemplateChild("PART_HorizontalScrollBar") as ScrollBar;
            if (_horizontalScrollBar != null)
            {
                _horizontalScrollBar.Scroll += (s, e) => SetHorizontalOffset(e.NewValue);
            }

            UpdateScrollMetrics();
        }

        public void SetVerticalOffset(double offset)
        {
            double max = Math.Max(0, _scrollData.Extent.Height - _scrollData.Viewport.Height);
            offset = Math.Max(0, Math.Min(offset, max));

            if (Math.Abs(_scrollData.Offset.Y - offset) > 0.001)
            {
                _scrollData.Offset.Y = offset;
                if (_verticalScrollBar != null && Math.Abs(_verticalScrollBar.Value - offset) > 0.001)
                    _verticalScrollBar.Value = offset;

                InvalidateVisual();
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            double max = Math.Max(0, _scrollData.Extent.Width - _scrollData.Viewport.Width);
            offset = Math.Max(0, Math.Min(offset, max));

            if (Math.Abs(_scrollData.Offset.X - offset) > 0.001)
            {
                _scrollData.Offset.X = offset;
                if (_horizontalScrollBar != null && Math.Abs(_horizontalScrollBar.Value - offset) > 0.001)
                    _horizontalScrollBar.Value = offset;

                InvalidateVisual();
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                // Shift + Wheel -> Horizontal Scroll
                double delta = (e.Delta / 120.0) * 30; // 30px per notch
                SetHorizontalOffset(HorizontalOffset - delta);
                e.Handled = true;
            }
            else
            {
                // Wheel -> Vertical Scroll
                double delta = (e.Delta / 120.0) * 3; // 3 lines per notch
                SetVerticalOffset(VerticalOffset - delta);
                e.Handled = true;
            }
            base.OnPreviewMouseWheel(e);
        }

        // Обновить метод UpdateScrollMetrics
        protected void UpdateScrollMetrics()
        {
            // Получаем настройки видимости, заданные в XAML на самом контроле
            var vertVis = ScrollViewer.GetVerticalScrollBarVisibility(this);
            var horzVis = ScrollViewer.GetHorizontalScrollBarVisibility(this);

            // 1. VERTICAL
            if (_verticalScrollBar != null)
            {
                double maxV = Math.Max(0, _scrollData.Extent.Height - _scrollData.Viewport.Height);
                _verticalScrollBar.Maximum = maxV;
                _verticalScrollBar.ViewportSize = _scrollData.Viewport.Height;
                _verticalScrollBar.Value = _scrollData.Offset.Y;

                // Логика видимости:
                // Visible -> Всегда показывать (даже если неактивен)
                // Hidden -> Всегда скрывать
                // Auto -> Показывать, если нужен
                // Disabled -> Скрывать и запрещать (можно доработать, пока как Hidden)

                if (vertVis == ScrollBarVisibility.Visible)
                    _verticalScrollBar.Visibility = Visibility.Visible;
                else if (vertVis == ScrollBarVisibility.Hidden || vertVis == ScrollBarVisibility.Disabled)
                    _verticalScrollBar.Visibility = Visibility.Collapsed;
                else // Auto
                    _verticalScrollBar.Visibility = maxV > 0 ? Visibility.Visible : Visibility.Collapsed;

                // Если Disabled, можно блокировать SetVerticalOffset, но это опционально
            }

            // 2. HORIZONTAL
            if (_horizontalScrollBar != null)
            {
                double maxH = Math.Max(0, _scrollData.Extent.Width - _scrollData.Viewport.Width);
                _horizontalScrollBar.Maximum = maxH;
                _horizontalScrollBar.ViewportSize = _scrollData.Viewport.Width;
                _horizontalScrollBar.Value = _scrollData.Offset.X;

                if (horzVis == ScrollBarVisibility.Visible)
                    _horizontalScrollBar.Visibility = Visibility.Visible;
                else if (horzVis == ScrollBarVisibility.Hidden || horzVis == ScrollBarVisibility.Disabled)
                    _horizontalScrollBar.Visibility = Visibility.Collapsed;
                else // Auto
                    _horizontalScrollBar.Visibility = maxH > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
}
