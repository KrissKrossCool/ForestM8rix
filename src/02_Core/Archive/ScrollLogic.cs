//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Input;
//using System.Windows.Media;


//namespace ForestM8rix;


//public class ScrollData
//{
//    public ScrollViewer ScrollOwner;
//    public Vector Offset;
//    public Size Extent;
//    public Size Viewport;
//}


//public abstract class ScrollLogic : Control, IScrollInfo
//{
//    protected ScrollData _scrollData = new ScrollData();
//    private ScrollViewer _scrollOwner;

//    public bool CanVerticallyScroll { get; set; } = true;
//    public bool CanHorizontallyScroll { get; set; } = true;

//    public double ExtentHeight => _scrollData.Extent.Height;
//    public double ViewportHeight => _scrollData.Viewport.Height;
//    public double VerticalOffset => _scrollData.Offset.Y;

//    public double ExtentWidth => _scrollData.Extent.Width;
//    public double ViewportWidth => _scrollData.Viewport.Width;
//    public double HorizontalOffset => _scrollData.Offset.X;

//    // [FIX] Превращаем в полноценное свойство
//    public ScrollViewer ScrollOwner
//    {
//        get => _scrollOwner;
//        set
//        {
//            _scrollOwner = value;
//            // Как только ScrollViewer "подключился", сообщаем ему наши размеры
//            _scrollOwner?.InvalidateScrollInfo();
//        }
//    }

//    public void SetVerticalOffset(double offset)
//    {
//        // 1. Проверяем, есть ли вообще куда скроллить.
//        // Если контента меньше, чем экрана, offset всегда 0.
//        if (ViewportHeight >= ExtentHeight)
//        {
//            offset = 0;
//        }
//        else
//        {
//            // 2. Ограничиваем offset в диапазоне [0, max]
//            // max = общая высота - видимая высота
//            double maxOffset = ExtentHeight - ViewportHeight;
//            offset = Math.Max(0, Math.Min(offset, maxOffset));
//        }

//        // 3. Применяем значение, только если оно реально изменилось
//        if (Math.Abs(_scrollData.Offset.Y - offset) > 0.001)
//        {
//            // 3.1. Сохраняем новое смещение
//            _scrollData.Offset.Y = offset;

//            // 3.2. Сообщаем "хозяину" (ScrollViewer), чтобы он обновил ползунок
//            ScrollOwner?.InvalidateScrollInfo();

//            // 3.3. Просим WPF перерисовать наш контрол (вызвать OnRender)
//            InvalidateVisual();
//        }
//    }


//    public void SetHorizontalOffset(double offset)
//    {
//        double maxOffset = Math.Max(0, ExtentWidth - ViewportWidth);
//        offset = Math.Max(0, Math.Min(offset, maxOffset));

//        if (Math.Abs(_scrollData.Offset.X - offset) > 0.001)
//        {
//            _scrollData.Offset.X = offset;
//            ScrollOwner?.InvalidateScrollInfo();
//            InvalidateVisual();
//        }
//    }

//    // [FIX] Инвертируем логику
//    public void LineUp() => SetVerticalOffset(VerticalOffset - 1);
//    public void LineDown() => SetVerticalOffset(VerticalOffset + 1);
//    public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
//    public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
//    public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - SystemParameters.WheelScrollLines);
//    public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + SystemParameters.WheelScrollLines);

//    // Горизонтальный скролл (оставляем как есть, обычно он не инвертирован)
//    public void LineLeft() => SetHorizontalOffset(HorizontalOffset - 10);
//    public void LineRight() => SetHorizontalOffset(HorizontalOffset + 10);
//    public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
//    public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);
//    public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - 30);
//    public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + 30);

//    public Rect MakeVisible(Visual visual, Rect rectangle) => Rect.Empty;
//}


