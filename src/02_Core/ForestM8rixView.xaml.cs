//// [ПОЛНЫЙ] UI/ForestM8rixView.xaml.cs
//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media;

//namespace ForestM8rix
//{
//    /// <summary>
//    /// Часть класса, отвечающая за связь с XAML-шаблоном (TemplateParts).
//    /// </summary>
//    public partial class ForestM8rixView2
//    {
//        private FrameworkElement _scrollContent;
//        private ScrollViewer _scrollViewer;
//        private ForestCanvas _internalCanvas;

//        public override void OnApplyTemplate()
//        {
//            base.OnApplyTemplate();
//            _scrollContent = GetTemplateChild("PART_ScrollContent") as FrameworkElement;
//            _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

//            if (_scrollViewer != null)
//            {
//                _scrollViewer.ScrollChanged += (s, e) => {
//                    Manager?.HandleScroll(_scrollViewer.VerticalOffset, _scrollViewer.HorizontalOffset);
//                };
//            }
//            Manager?.UpdateVisualParams();
//        }

//        public  void OnApplyTemplate2()
//        {
//            base.OnApplyTemplate();

//            // 1. Ищем ScrollViewer для управления прокруткой
//            _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

//            // 2. Ищем и настраиваем холст отрисовки
//            if (GetTemplateChild("PART_Canvas") is Canvas placeholder)
//            {
//                _internalCanvas = new ForestCanvas { OwnerManager = _manager };

//                // Если в шаблоне был заглушечный Canvas, заменяем его контент или кладем сверху
//                if (_scrollViewer != null)
//                {
//                    _scrollViewer.Content = _internalCanvas;
//                }

//                // Подписка на клики именно по холсту (для точности координат)
//                _internalCanvas.MouseDown += (s, e) =>
//                {
//                    this.Focus();
//                    _manager.HandleMouseDown(
//                        e.GetPosition(_internalCanvas),
//                        Keyboard.Modifiers.HasFlag(ModifierKeys.Control),
//                        Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
//                };
//            }

//            // 3. Синхронизация прокрутки с DisplayService
//            if (_scrollViewer != null)
//            {
//                _scrollViewer.ScrollChanged += (s, e) =>
//                {
//                    // Сообщаем менеджеру новые оффсеты (в индексах строк и пикселях X)
//                    double rowH = _manager.Display.RowHeight * _manager.Scale;
//                    _manager.HandleScroll(e.VerticalOffset / rowH, e.HorizontalOffset);
//                };
//            }

//            // Инициализируем визуальные параметры (шрифт, DPI)
//            _manager.UpdateVisualParams();
//        }

//        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
//        {
//            base.OnDpiChanged(oldDpi, newDpi);
//            _manager.RequestRender();
//        }
//    }
//}