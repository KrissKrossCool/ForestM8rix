using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace ForestM8rix
{
    public class ForestM8rixView : ScrollLogic
    {
        public ForestM8rixManager Manager => _manager;
        private readonly ForestM8rixManager _manager;
        public double RowHeight => _manager.RowHeight;

        static ForestM8rixView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForestM8rixView),
                new FrameworkPropertyMetadata(typeof(ForestM8rixView)));
        }

        public ForestM8rixView()
        {
            _manager = new ForestM8rixManager(this);
            this.Background = Brushes.Transparent;

            // СУТЬ: Подписка на туннельное событие (спускается сверху вниз)
            this.PreviewMouseDown += (s, e) => {
                System.Diagnostics.Debug.WriteLine("CLICK DETECTED!");      };

            // СУТЬ: Используем туннельное событие, которое никто не успеет перехватить
            this.PreviewMouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Point clickPoint = e.GetPosition(this);

                    // 1. Отправляем в менеджер
                    _manager.HandleClick(clickPoint);

                    // 2. Форсируем перерисовку
                    InvalidateVisual();

                    // Опционально: e.Handled = true; // Если не хотим, чтобы клик шел дальше
                }
            };
        }

        private void OnViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point clickPoint = e.GetPosition(this);

            // СУТЬ: Преобразование экранных координат в логические координаты данных
            // С учетом того, что VerticalOffset в ScrollLogic у нас в "строках"
            double logicalY = clickPoint.Y + (VerticalOffset * RowHeight);

            Point logicalPoint = new Point(clickPoint.X, logicalY);

            _manager.HandleClick(logicalPoint);
        }

        protected void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // Получаем точку клика относительно контрола
            Point clickPoint = e.GetPosition(this);

            // Передаем координаты в менеджер. 
            // Он сам вычислит, попали ли мы в строку или в треугольник экспандера.
            _manager.HandleClick(clickPoint);

            // Принудительно перерисовываем, чтобы увидеть изменения (открытую папку)
            InvalidateVisual();
        }

        public void SetData(IEnumerable source, Func<object, IEnumerable> selector)
        {
            _manager.SetSource(source, selector);
            UpdateScrollMetrics(); // СИНХРОНИЗАЦИЯ: Обновляем Extent при смене данных
            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateScrollMetrics(); // СИНХРОНИЗАЦИЯ: Обновляем Viewport при ресайзе
        }

        private void UpdateScrollMetrics()
        {
            _scrollData.Extent.Height = _manager.Count; // В строках
            _scrollData.Viewport.Height = Math.Floor(ActualHeight / RowHeight);
            ScrollOwner?.InvalidateScrollInfo();
        }

        protected override void OnRender(DrawingContext dc)
        {
            // [СУТЬ] Если вы здесь видите старый код с DrawText — удалите его!
            // Этот метод должен ТОЛЬКО вызывать Manager.Render

            // Временная проверка: рисуем маленький синий квадрат, чтобы понять, что МЫ ТУТ
            dc.DrawRectangle(Brushes.Blue, null, new Rect(0, 0, 50, 50));

            _manager.Render(dc, new Size(ActualWidth, ActualHeight));
        }

        private void RenderVisibleNodes(DrawingContext dc)
        {
            double indentStep = 20.0;

            // СУТЬ: Индекс первой видимой строки из ScrollLogic
            int firstVisibleIndex = (int)VerticalOffset;
            int lastVisibleIndex = firstVisibleIndex + (int)_scrollData.Viewport.Height + 1;

            // Ограничиваем цикл только видимым диапазоном (Виртуализация отрисовки)
            for (int i = firstVisibleIndex; i <= lastVisibleIndex && i < _manager.Count; i++)
            {
                // Построчный сдвиг: i - VerticalOffset всегда даст 0 для первой видимой строки
                double yPos = (i - VerticalOffset) * RowHeight;

                double xPos = (_manager.Levels[i] * indentStep) + 10;
                object node = _manager.Nodes[i];

                if (_manager.HasChildren(node))
                {
                    bool isExpanded = StateManagement.ForestStateRegistry.IsExpanded(node);
                    DrawText(dc, isExpanded ? "▼" : "▶", xPos - 12, yPos, Brushes.Gray);
                }

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