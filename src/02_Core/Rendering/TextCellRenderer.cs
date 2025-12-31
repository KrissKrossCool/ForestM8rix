// [ПОЛНЫЙ]
using System.Windows;
using System.Windows.Media;
using ForestM8rix.Core;
using ForestM8rix.Columns;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Rendering.CellRenderers
{
    public class TextCellRenderer : ICellRenderer
    {
        private readonly ForestM8rixManager _manager;
        private readonly ForestM8rixColumn _column;

        public TextCellRenderer(ForestM8rixManager manager, ForestM8rixColumn column)
        {
            _manager = manager;
            _column = column;
        }

        public void Draw(DrawingContext dc, Rect rect, object node, bool isSelected)
        {
            if (node == null) return;

            // 1. [TAG] Отрисовка экспандера (только если это первая колонка в списке)
            bool isFirstColumn = _manager.Columns.IndexOf(_column) == 0;
            double currentX = rect.X;

            if (isFirstColumn)
            {
                int level = _manager.GetLevel(node);
                double indent = level * _manager.IndentSize * _manager.Scale;
                currentX += indent;

                // Рисуем треугольник, если у узла есть дети
                if (_manager.HasChildren(node))
                {
                    bool isExpanded = ForestStateRegistry.IsExpanded(node);
                    DrawExpander(dc, new Point(currentX, rect.Y), rect.Height, isExpanded);
                }

                // Сдвигаем текст вправо от области экспандера
                currentX += 20 * _manager.Scale;
            }

            // 2. [TAG] Получение текста через логику колонки
            string text = _column.GetText(node);

            Brush textBrush = isSelected
                ? ForestM8rixOptions.GetSelectionForeground(_manager.View)
                : ForestM8rixOptions.GetNormalForeground(_manager.View);

            FormattedText ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _manager.DefaultTypeface,
                12 * _manager.Scale,
                textBrush,
                _manager.Dpi);

            ft.MaxTextWidth = Math.Max(1, rect.Right - currentX - 5);
            ft.MaxTextHeight = rect.Height;
            ft.Trimming = TextTrimming.CharacterEllipsis;

            double verticalOffset = (rect.Height - ft.Height) / 2;
            dc.DrawText(ft, new Point(currentX + 5, rect.Y + verticalOffset));
        }

        private void DrawExpander(DrawingContext dc, Point startPoint, double rowHeight, bool isExpanded)
        {
            double size = 10 * _manager.Scale;
            double centerX = startPoint.X + 10 * _manager.Scale;
            double centerY = startPoint.Y + rowHeight / 2;

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                if (isExpanded) // Рисуем раскрытый (вниз)
                {
                    context.BeginFigure(new Point(centerX - size / 2, centerY - size / 4), true, true);
                    context.LineTo(new Point(centerX + size / 2, centerY - size / 4), true, false);
                    context.LineTo(new Point(centerX, centerY + size / 4), true, false);
                }
                else // Рисуем закрытый (вправо)
                {
                    context.BeginFigure(new Point(centerX - size / 4, centerY - size / 2), true, true);
                    context.LineTo(new Point(centerX - size / 4, centerY + size / 2), true, false);
                    context.LineTo(new Point(centerX + size / 4, centerY), true, false);
                }
            }
            dc.DrawGeometry(Brushes.Gray, null, geometry);
        }
    }
}