using System.Windows;
using System.Windows.Media;

namespace ForestM8rix.Columns
{
    public class TemplateHeaderRenderer : IHeaderRenderer
    {
        private readonly ForestM8rixColumn _column;

        public TemplateHeaderRenderer(ForestM8rixColumn column)
        {
            _column = column;
        }

        public void Draw(DrawingContext dc, Rect rect)
        {
            if (_column.HeaderTemplate == null) return;

            // 1. Создаем визуальный элемент из "чертежа"
            var content = (FrameworkElement)_column.HeaderTemplate.LoadContent();

            // 2. Устанавливаем данные (DataContext)
            content.DataContext = _column;

            // 3. Заставляем WPF рассчитать размеры элемента
            content.Measure(rect.Size);
            content.Arrange(new Rect(rect.Size));
            content.UpdateLayout();

            // 4. Превращаем UIElement в "кисть" и рисуем прямоугольник этой кистью
            var brush = new VisualBrush(content);
            dc.DrawRectangle(brush, null, rect);
        }
    }
}