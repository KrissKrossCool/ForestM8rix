using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ForestM8rix
{
    public abstract class ScrollLogic : Control, IScrollInfo
    {
        protected ScrollData _scrollData = new ScrollData();

        // Реализация интерфейса IScrollInfo
        public bool CanVerticallyScroll { get; set; } = true;
        public bool CanHorizontallyScroll { get; set; } = false;

        public double ExtentHeight => _scrollData.Extent.Height;
        public double ViewportHeight => _scrollData.Viewport.Height;
        public double VerticalOffset => _scrollData.Offset.Y;

        public ScrollViewer ScrollOwner
        {
            get => _scrollData.ScrollOwner;
            set => _scrollData.ScrollOwner = value;
        }

        public void SetVerticalOffset(double offset)
        {
            // Здесь будет логика ограничения и InvalidateVisual
            _scrollData.Offset.Y = offset;
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateVisual();
        }

        // Заглушки для корректной компиляции интерфейса
        public void LineUp() => SetVerticalOffset(VerticalOffset - 1);
        public void LineDown() => SetVerticalOffset(VerticalOffset + 1);
        public void PageUp() { }
        public void PageDown() { }
        public void MouseWheelUp() => LineUp();
        public void MouseWheelDown() => LineDown();
        public void SetHorizontalOffset(double offset) { }
        public double ExtentWidth => 0;
        public double ViewportWidth => 0;
        public double HorizontalOffset => 0;
        public void LineLeft() { }
        public void LineRight() { }
        public void PageLeft() { }
        public void PageRight() { }
        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }
        public Rect MakeVisible(Visual visual, Rect rectangle) => Rect.Empty;
    }
}