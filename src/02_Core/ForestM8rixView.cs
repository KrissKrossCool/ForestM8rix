using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

            System.Diagnostics.Debug.WriteLine("--> [CONSTRUCTOR] ForestM8rixView created.");

            this.Background = Brushes.Transparent;
            this.Focusable = true;
           // _manager.RequestScrollIntoView += ScrollRowIntoView;
        }

        // --- ИСХОДЯЩАЯ СИНХРОНИЗАЦИЯ (VIEW -> VM) ---
        public void OnSelectionUpdated()
        {
            SelectionData.SetOneItem(this, _manager.LastSelectedNode);
            var list = SelectionData.GetList(this);
            if (list != null && !list.IsReadOnly)
            {
                try
                {
                    list.Clear();
                    foreach (var item in _manager.GetSelectedItems()) list.Add(item);
                }
                catch { /* Игнорируем */ }
            }
        }

        // --- ВВОД: КЛАВИАТУРА ([FIX] Используем PreviewKeyDown) ---
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Перехватываем только нужные нам клавиши
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.A: // Для Ctrl+A
                    _manager.HandleKeyDown(e.Key);
                    e.Handled = true; // Важно: блокируем дальнейшую обработку
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        // --- ВВОД: МЫШЬ (без изменений) ---
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Focus();
                _manager.VerticalOffset = this.VerticalOffset;
                _manager.HorizontalOffset = this.HorizontalOffset;
                Point p = e.GetPosition(this);
                _dragStartPoint = p;
                _isDraggingSelection = false;
                _manager.HandleClick(p);
                CaptureMouse();
            }
        }

        // ... (OnMouseMove, OnMouseUp, OnMouseLeave без изменений) ...
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _manager.VerticalOffset = this.VerticalOffset;
            _manager.HorizontalOffset = this.HorizontalOffset;
            if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                Vector diff = _dragStartPoint - current;
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
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
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

        // --- SCROLL & RENDER LOGIC ---

        private void ScrollRowIntoView(int rowIndex)
        {
            // Безопасность
            if (RowHeight < double.Epsilon || ActualHeight < double.Epsilon) return;

            // [FIX] Переходим на более простую логику.
            // ViewportHeight из базового класса должен быть доступен, если там public
            double viewportHeightInRows = _scrollData.Viewport.Height;

            // Если целевая строка выше текущей видимой области
            if (rowIndex < VerticalOffset)
            {
                // Ставим её на самый верх
                SetVerticalOffset(rowIndex);
            }
            // Если целевая строка ниже текущей видимой области
            // (Проверяем, что rowIndex за пределами [Offset, Offset + Viewport-1])
            else if (rowIndex >= VerticalOffset + viewportHeightInRows)
            {
                // Ставим её на самый низ
                SetVerticalOffset(rowIndex - viewportHeightInRows + 1);
            }
        }

        // 1. Метод переименован, чтобы не конфликтовать с базовым
        private void UpdateMetrics()
        {
            _scrollData.Extent.Height = _manager.Count;
            _scrollData.Viewport.Height = (RowHeight > 0) ? Math.Floor(ActualHeight / RowHeight) : 0;
            _scrollData.Extent.Width = _manager.TotalWidth;
            _scrollData.Viewport.Width = ActualWidth;
        }

        // 2. Везде, где был UpdateScrollMetrics, делаем так:
        //private void OnSizeChangedOrDataLoaded()
        //{
        //    UpdateScrollMetrics(); // Сначала обновляем цифры
        //    base.UpdateScrollMetrics(); // Потом просим базу обновить UI скроллов
        //}

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            UpdateMetrics(); // Сначала наши цифры
          //  base.UpdateScrollMetrics(); // Потом UI скроллов
        }

        protected override void OnRender(DrawingContext dc)
        {

            System.Diagnostics.Debug.WriteLine($"[RENDER] Drawing frame. Passing Offset Y: {this.VerticalOffset:F2}");

            _manager.VerticalOffset = this.VerticalOffset;
            _manager.HorizontalOffset = this.HorizontalOffset;
            _manager.Render(dc, new Size(ActualWidth, ActualHeight));
        }

        public void SetData(IEnumerable source, Func<object, IEnumerable> selector)
        {
            _manager.SetSource(source, selector);

            UpdateMetrics(); // Сначала наши цифры
          //  base.UpdateScrollMetrics(); // Потом UI скроллов
        }
    }
}
