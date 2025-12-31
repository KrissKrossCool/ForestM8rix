using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ForestM8rix.Rendering
{
    public static class ForestRenderHelper
    {
        // [TAG] Векторные данные иконок (Path Data)
        private static readonly Geometry IconNode = Geometry.Parse("M2,6 L12,1 L22,6 L22,18 L12,23 L2,18 Z M12,1 L12,12 M2,6 L12,12 L22,6");
        private static readonly Geometry IconFolder = Geometry.Parse("M2,4 H9 L11,6 H22 V20 H2 Z");
        private static readonly Geometry IconFile = Geometry.Parse("M4,2 H14 L20,8 V22 H4 Z M14,2 V8 H20");

        /// <summary>
        /// Отрисовка текста ячейки с учетом масштаба и обрезки
        /// </summary>
        public static void DrawCellText(DrawingContext dc, string text, Rect rect, Typeface tf, double fontSize, Brush color, double dpi)
        {
            if (rect.Width <= 5 || string.IsNullOrEmpty(text)) return;

            var ft = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                tf,
                fontSize,
                color,
                dpi);

            ft.MaxTextWidth = Math.Max(1, rect.Width - 5);
            ft.MaxTextHeight = rect.Height;
            ft.Trimming = TextTrimming.CharacterEllipsis;

            dc.PushClip(new RectangleGeometry(rect));
            dc.DrawText(ft, new Point(rect.X + 5, rect.Y + (rect.Height - ft.Height) / 2));
            dc.Pop();
        }

        /// <summary>
        /// Отрисовка пунктирных линий дерева (Tree Lines)
        /// </summary>
        public static void DrawTreeLines(DrawingContext dc, int level, double x, double y, double rowHeight, double indent, bool isLast, bool[] parentsHasNext)
        {
            Pen linePen = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 0.8)
            {
                DashStyle = DashStyles.Dot
            };

            double midY = y + rowHeight / 2;
            double centerX = x + (level * indent) + indent / 2;

            // 1. Горизонтальное "плечо" к узлу
            dc.DrawLine(linePen, new Point(centerX, midY), new Point(centerX + indent / 2, midY));

            // 2. Вертикальная линия текущего уровня (L-образная если последний ребенок)
            dc.DrawLine(linePen, new Point(centerX, y), new Point(centerX, isLast ? midY : y + rowHeight));

            // 3. Вертикальные транзитные линии для уровней выше
            if (parentsHasNext != null)
            {
                for (int i = 0; i < level; i++)
                {
                    if (i < parentsHasNext.Length && parentsHasNext[i])
                    {
                        double tx = x + (i * indent) + indent / 2;
                        dc.DrawLine(linePen, new Point(tx, y), new Point(tx, y + rowHeight));
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовка векторной иконки с масштабированием
        /// </summary>
        public static void DrawIcon(DrawingContext dc, Rect rect, string iconKey, Brush fill)
        {
            Geometry geo = iconKey switch
            {
                "Node" => IconNode,
                "Folder" => IconFolder,
                "File" => IconFile,
                _ => IconFile
            };

            if (geo == null) return;

            // Сохраняем состояние для трансформации
            dc.PushTransform(new TranslateTransform(rect.X, rect.Y));

            // Исходный размер геометрий 24x24, масштабируем под целевой Rect
            double scale = rect.Width / 24.0;
            dc.PushTransform(new ScaleTransform(scale, scale));

            dc.DrawGeometry(fill, null, geo);

            dc.Pop(); // Pop Scale
            dc.Pop(); // Pop Translate
        }

        /// <summary>
        /// Отрисовка экспандера (треугольника)
        /// </summary>
        public static void DrawExpander(DrawingContext dc, Point center, bool isExpanded, Brush color)
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
            dc.DrawGeometry(color, null, geometry);
        }
    }
}