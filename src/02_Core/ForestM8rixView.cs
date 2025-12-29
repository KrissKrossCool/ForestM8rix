using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

namespace ForestM8rix
{
    public class ForestM8rixView : Control
    {
        private readonly ForestM8rixManager _manager;
        public double RowHeight => _manager.RowHeight;

        public ForestM8rixView()
        {
            _manager = new ForestM8rixManager(this);

            // [СУТЬ] Чтобы Background из XAML работал, нужно разрешить его отрисовку
            // Иначе Control может игнорировать OnRender фоном
            this.Background = Brushes.White;

            // Обработка клика для раскрытия/свертывания
            this.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _manager.HandleClick(e.GetPosition(this));
                }
            };
        }

        public void SetData(IEnumerable source, Func<object, IEnumerable> selector)
        {
            _manager.SetSource(source, selector);
            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Расчет желаемой высоты на основе количества строк
            double desiredHeight = _manager.Count * RowHeight;

            // Защита от 0 и NaN для корректной работы Layout
            if (desiredHeight <= 0) desiredHeight = 1;

            double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
            double height = double.IsInfinity(availableSize.Height) ? desiredHeight : availableSize.Height;

            return new Size(width, height);
        }

        protected override void OnRender(DrawingContext dc)
        {
            // Отрисовка фона
            dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (_manager.Count == 0)
            {
                DrawText(dc, "ДАННЫХ НЕТ (Manager.Count == 0)", 10, 10, Brushes.Red);
                return;
            }

            RenderVisibleNodes(dc);
        }

        private void RenderVisibleNodes(DrawingContext dc)
        {
            double indentStep = 20.0;

            for (int i = 0; i < _manager.Count; i++)
            {
                double yPos = (i * RowHeight) - _manager.VerticalOffset;

                // Отрисовка только видимых элементов (куллинг)
                if (yPos + RowHeight < 0) continue;
                if (yPos > ActualHeight) break;

                double xPos = (_manager.Levels[i] * indentStep) + 10;
                object node = _manager.Nodes[i];

                // Отрисовка маркера раскрытия (Expand/Collapse)
                if (_manager.HasChildren(node))
                {
                    bool isExpanded = StateManagement.ForestStateRegistry.IsExpanded(node);
                    DrawText(dc, isExpanded ? "▼" : "▶", xPos - 12, yPos, Brushes.Gray);
                }

                // Отрисовка текста узла
                string content = node?.ToString() ?? "NULL";
                DrawText(dc, content, xPos, yPos, Brushes.Black);
            }
        }

        private void DrawText(DrawingContext dc, string text, double x, double y, Brush color)
        {
            if (string.IsNullOrEmpty(text)) return;

            var ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                12,
                color,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(ft, new Point(x, y));
        }
    }
}