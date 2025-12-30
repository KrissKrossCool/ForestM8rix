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
            _manager.RequestScrollIntoView += ScrollRowIntoView;
        }

        // --- ИСХОДЯЩАЯ СИНХРОНИЗАЦИЯ (VIEW -> VM) ---
        public void OnSelectionUpdated()
        {
            SelectionData.SetOneItem(this, _manager.LastSelectedNode);
            //var list = SelectionData.GetList(this);
            //if (list != null && !list.IsReadOnly)
            //{
            //    try
            //    {
            //        list.Clear();
            //        foreach (var item in _manager.GetSelectedItems()) list.Add(item);
            //    }
            //    catch { /* Игнорируем */ }
            //}
        }

        // ЗАМЕНИТЬ OnPreviewKeyDown
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                // ВСЕ клавиши навигации/выделения идут в Менеджер
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.A when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    _manager.HandleKeyDown(e.Key);
                    e.Handled = true;
                    break;

                // PageUp/Down - это чистый скролл, пусть IScrollInfo работает
                case Key.PageUp:
                    PageUp();
                    e.Handled = true;
                    break;
                case Key.PageDown:
                    PageDown();
                    e.Handled = true;
                    break;
            }
        }



        // ЗАМЕНИТЬ UpdateScrollMetrics
        private void UpdateScrollMetrics()
        {
            _scrollData.Extent.Height = _manager.Count;
            _scrollData.Viewport.Height = (RowHeight > 0) ? Math.Floor(ActualHeight / RowHeight) : 0;
            _scrollData.Extent.Width = _manager.TotalWidth;
            _scrollData.Viewport.Width = ActualWidth;

            // Просто сообщаем "хозяину", что цифры поменялись
            ScrollOwner?.InvalidateScrollInfo();
        }


        // ЗАМЕНИТЬ ScrollRowIntoView
        private void ScrollRowIntoView(int rowIndex)
        {
            if (RowHeight < double.Epsilon || ActualHeight < double.Epsilon) return;

            double viewportHeightInRows = ViewportHeight;
            if (viewportHeightInRows < 1) viewportHeightInRows = 1;

            // Если строка выше видимой области
            if (rowIndex < VerticalOffset)
            {
                SetVerticalOffset(rowIndex);
            }
            // [FIX] Если строка ниже или равна последней видимой
            else if (rowIndex >= VerticalOffset + viewportHeightInRows)
            {
                SetVerticalOffset(rowIndex - viewportHeightInRows + 1);
            }
        }



        // --- ВВОД: МЫШЬ (без изменений) ---
        //protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        //{
        //    base.OnPreviewMouseDown(e);
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        this.Focus();
        //        _manager.VerticalOffset = this.VerticalOffset;
        //        _manager.HorizontalOffset = this.HorizontalOffset;
        //        Point p = e.GetPosition(this);
        //        _dragStartPoint = p;
        //        _isDraggingSelection = false;
        //        _manager.HandleClick(p);
        //        CaptureMouse();
        //    }
        //}

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // 1. Агрессивно забираем фокус на себя
            if (this.Focusable && !this.IsFocused)
            {
                this.Focus();
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                // Синхронизация
                _manager.VerticalOffset = this.VerticalOffset;
                _manager.HorizontalOffset = this.HorizontalOffset;

                Point p = e.GetPosition(this);
                _dragStartPoint = p;
                _isDraggingSelection = false;

                _manager.HandleClick(p);

                // Захват мыши для драга
                CaptureMouse();

                // 2. [FIX] Говорим системе, что мы обработали клик.
                // Фокус останется здесь, и OnPreviewKeyDown будет работать.
                e.Handled = true;
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

        //private void ScrollRowIntoView(int rowIndex)
        //{
        //    // Безопасность
        //    if (RowHeight < double.Epsilon || ActualHeight < double.Epsilon) return;

        //    // [FIX] Переходим на более простую логику.
        //    // ViewportHeight из базового класса должен быть доступен, если там public
        //    double viewportHeightInRows = _scrollData.Viewport.Height;

        //    // Если целевая строка выше текущей видимой области
        //    if (rowIndex < VerticalOffset)
        //    {
        //        // Ставим её на самый верх
        //        SetVerticalOffset(rowIndex);
        //    }
        //    // Если целевая строка ниже текущей видимой области
        //    // (Проверяем, что rowIndex за пределами [Offset, Offset + Viewport-1])
        //    else if (rowIndex >= VerticalOffset + viewportHeightInRows)
        //    {
        //        // Ставим её на самый низ
        //        SetVerticalOffset(rowIndex - viewportHeightInRows + 1);
        //    }
        //}

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
            
            UpdateScrollMetrics();
            //UpdateMetrics(); // Сначала наши цифры
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

            // [ВОССТАНОВИТЬ] Обновляем метрики после загрузки данных
            UpdateScrollMetrics();
            InvalidateVisual();
        }
    }
}
