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
        }

        public void SetData(IEnumerable source, Func<object, IEnumerable> selector)
        {
            _manager.SetSource(source, selector);
            UpdateScrollMetrics();
            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                this.Focus();
                _manager.VerticalOffset = this.VerticalOffset; // Синхронизация

                Point p = e.GetPosition(this);

                // 1. Сначала ВСЕГДА запоминаем точку старта (даже если драга не будет)
                _dragStartPoint = p;
                _isDraggingSelection = false;

                // 2. Обрабатываем клик (Выделение строки)
                _manager.HandleClick(p);

                // 3. Захватываем мышь на случай, если пользователь начнет тянуть
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _manager.VerticalOffset = this.VerticalOffset; // Синхронизация

            if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                Vector diff = _dragStartPoint - current;

                // СУТЬ: Проверяем порог драга (5px) И включен ли режим рамки
                if ((Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
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

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // СУТЬ: Гарантированный сброс всех состояний
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateScrollMetrics();
        }

        private void UpdateScrollMetrics()
        {
            _scrollData.Extent.Height = _manager.Count;
            _scrollData.Viewport.Height = Math.Floor(ActualHeight / RowHeight);
            ScrollOwner?.InvalidateScrollInfo();
        }

        protected override void OnRender(DrawingContext dc)
        {
            _manager.VerticalOffset = this.VerticalOffset;
            _manager.Render(dc, new Size(ActualWidth, ActualHeight));
        }
    }
}
