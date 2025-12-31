using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ForestM8rix.Columns;

namespace ForestM8rix.Rendering;

public class ForestM8rixHeader : FrameworkElement
{
    // Свойство для управления фоном всей полосы заголовка
    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(ForestM8rixHeader),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    // Свойство для передачи списка колонок (поддерживает Binding)
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(IEnumerable<ForestM8rixColumn>), typeof(ForestM8rixHeader),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    // Свойство для синхронизации с горизонтальным скроллом
    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(ForestM8rixHeader),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty ScaleProperty =
DependencyProperty.Register(nameof(Scale), typeof(double), typeof(ForestM8rixHeader),
    new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IEnumerable<ForestM8rixColumn> Columns
    {
        get => (IEnumerable<ForestM8rixColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    // [СУТЬ]
    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (Columns == null) return;

        double currentX = -HorizontalOffset;

        foreach (var column in Columns)
        {
            double scaledWidth = column.Width * Scale;

            // [TAG] ПЕРЕДАЕМ scaledWidth, ЧТОБЫ ТЕКСТ НЕ СЪЕЗЖАЛ
            Rect rect = new Rect(currentX, 0, scaledWidth, ActualHeight);

            // Отрисовываем, если колонка хоть частично видна
            if (currentX + scaledWidth > 0 && currentX < ActualWidth)
            {
                var render  = column.GetHeaderRenderer(this);
                render.Draw(dc, rect);
            }

            currentX += scaledWidth;
        }
    }

    //protected override void OnRender(DrawingContext dc)
    //{
    //    // [TAG] Заливаем фон на всю доступную ширину окна (ActualWidth)
    //    dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

    //    if (Columns == null) return;

    //    var firstCol = Columns.First();
    //    double scaledWidth2 = firstCol.Width * Scale;

    //    System.Diagnostics.Debug.WriteLine($"[HEADER] Render Offset: {HorizontalOffset:F2} | Scale: {Scale:F2} | FirstCol ScaledWidth: {scaledWidth2:F2}");
    //    // Начинаем отрисовку с учетом смещения скролла
    //    double currentX = -HorizontalOffset;

    //    foreach (var column in Columns)
    //    {
    //        // Отрисовываем только те колонки, которые попадают в видимую область
    //        //if (currentX + column.Width > 0 && currentX < ActualWidth)
    //        //{

    //            // 2. ВНИМАНИЕ: Если дерево масштабируется (Scale), 
    //            // то и ширина колонки в шапке должна масштабироваться!
    //            double scaledWidth = column.Width * Scale;

    //            Rect rect = new Rect(currentX, 0, column.Width, ActualHeight);

    //            // Вызываем рендерер конкретной колонки
    //            column.GetHeaderRenderer().Draw(dc, rect);
    //       // }

    //        //currentX += column.Width;
    //        currentX += scaledWidth;
    //    }
    //}
}
