using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ForestM8rix.Columns;

namespace ForestM8rix.Rendering // [ВАЖНО] Проверьте, что папка Rendering совпадает
{
    public class ForestM8rixHeader : FrameworkElement
    {
        // Добавляем свойство Background, раз мы обратились к нему в Generic.xaml
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(ForestM8rixHeader),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public IEnumerable<ForestM8rixColumn> Columns { get; set; }

        protected override void OnRender(DrawingContext dc)
        {
            // Рисуем фон, раз он задан
            dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (Columns == null) return;
            double currentX = 0;

            foreach (var column in Columns)
            {
                Rect rect = new Rect(currentX, 0, column.Width, ActualHeight);
                column.GetHeaderRenderer().Draw(dc, rect);
                currentX += column.Width;
            }
        }
    }
}