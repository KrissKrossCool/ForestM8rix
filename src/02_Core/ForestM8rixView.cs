using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ForestM8rix
{
    public class ForestM8rixView : ScrollLogic
    {
        public ForestM8rixManager Manager => _manager;
        private readonly ForestM8rixManager _manager;
        public double RowHeight => _manager.RowHeight;

        private Point _dragStartPoint;
        private bool _isDraggingSelection;

        static ForestM8rixView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForestM8rixView),
                new FrameworkPropertyMetadata(typeof(ForestM8rixView)));
        }

        public ForestM8rixView()
        {
            _manager = new ForestM8rixManager(this);
            this.Background = Brushes.Transparent;
            this.Focusable = true;

            // Подписка на автоматическую прокрутку
            _manager.RequestScrollIntoView += ScrollRowIntoView;
        }

        public void SetData(IEnumerable source, Func<object, IEnumerable> selector)
        {
            _manager.SetSource(source, selector);
            UpdateScrollMetrics();
            InvalidateMeasure();
            InvalidateVisual();
        }

        // === ВВОД: КЛАВИАТУРА ===
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                _manager.HandleKeyDown(e.Key);
                e.Handled = true; // Перехватываем управление
            }
            base.OnKeyDown(e);
        }

        // === ВВОД: МЫШЬ (КЛИК) ===
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                this.Focus();

                // СИНХРОНИЗАЦИЯ
                _manager.VerticalOffset = this.VerticalOffset;
                _manager.HorizontalOffset = this.HorizontalOffset;

                Point p = e.GetPosition(this);

                // 1. Запоминаем старт
                _dragStartPoint = p;
                _isDraggingSelection = false;

                // 2. Обрабатываем клик
                _manager.HandleClick(p);

                // 3. Захват
                CaptureMouse();
            }
        }

        // === ВВОД: МЫШЬ (ДРАГ / ХОВЕР) ===
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // СИНХРОНИЗАЦИЯ
            _manager.VerticalOffset = this.VerticalOffset;
            _manager.HorizontalOffset = this.HorizontalOffset;

            if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                Vector diff = _dragStartPoint - current;

                // Логика рамки (Rubber Band)
                if (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5)
                {
                    if (_manager.IsRubberBandEnabled)
                    {
                        _isDraggingSelection = true;
                        _manager.UpdateSelectionRect(_dragStartPoint, current);
                    }
                }
            }
            else
            {
                base.OnMouseMove(e);
                _manager.HandleMouseMove(e.GetPosition(this));
            }
        }

        // === ВВОД: МЫШЬ (ОТПУСКАНИЕ) ===
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            if (_isDraggingSelection)
            {
                _manager.ClearSelectionRect();
                _isDraggingSelection = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsMouseCaptured) _manager.HandleMouseLeave();
        }

        // === ЛОГИКА СКРОЛЛА И РЕНДЕРА ===

        // Метод умной прокрутки к строке
        private void ScrollRowIntoView(int rowIndex)
        {
            if (RowHeight < double.Epsilon) return;
            if (ActualHeight < double.Epsilon) return;

            double viewportRows = Math.Floor(ActualHeight / RowHeight);
            if (viewportRows < 1.0) viewportRows = 1.0;

            if (rowIndex < VerticalOffset)
            {
                SetVerticalOffset(rowIndex);
            }
            else if (rowIndex >= VerticalOffset + viewportRows)
            {
                SetVerticalOffset(rowIndex - viewportRows + 1);
            }
        }

        // Обновление размеров скроллбаров
        private void UpdateScrollMetrics()
        {
            _scrollData.Extent.Height = _manager.Count;
            if (RowHeight > 0)
                _scrollData.Viewport.Height = Math.Floor(ActualHeight / RowHeight);

            _scrollData.Extent.Width = _manager.TotalWidth;
            _scrollData.Viewport.Width = ActualWidth;

            base.UpdateScrollMetrics();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateScrollMetrics();
        }

        protected override void OnRender(DrawingContext dc)
        {
            _manager.VerticalOffset = this.VerticalOffset;
            _manager.HorizontalOffset = this.HorizontalOffset;

            _manager.Render(dc, new Size(ActualWidth, ActualHeight));
        }
    }
}
