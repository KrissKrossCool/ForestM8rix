using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ForestM8rix.Columns
{
    public class TextHeaderRenderer : IHeaderRenderer
    {
        private readonly ForestM8rixColumn _column;

        public TextHeaderRenderer(ForestM8rixColumn column)
        {
            _column = column;
        }

        public void Draw(DrawingContext dc, Rect rect)
        {
            if (string.IsNullOrEmpty(_column.Title)) return;

            // Настраиваем шрифт (используем стандартные значения, чтобы не ломать код)
            var typeface = new Typeface(
                new FontFamily("Segoe UI"),
                FontStyles.Normal,
                FontWeights.SemiBold,
                FontStretches.Normal);

            var formattedText = new FormattedText(
                _column.Title,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12.0, // FontSize
                Brushes.Black,
                VisualTreeHelper.GetDpi(new UIElement()).PixelsPerDip);

            // Ограничиваем текст шириной колонки (обрезка)
            formattedText.MaxTextWidth = rect.Width > 0 ? rect.Width : 1;
            formattedText.MaxTextHeight = rect.Height;

            // Рисуем текст с небольшим отступом слева (Padding)
            dc.DrawText(formattedText, new Point(rect.X + 5, rect.Y + (rect.Height - formattedText.Height) / 2));
        }
    }
}