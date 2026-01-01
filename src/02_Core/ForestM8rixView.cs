// [ПОЛНЫЙ]
namespace ForestM8rix
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using ForestM8rix.Core;

    public partial class ForestM8rixView
    {
        private readonly ForestM8rixManager _manager;
        private Canvas _canvas;
        private ScrollViewer _scrollViewer;

        public ForestM8rixManager Manager => _manager;

        public ForestM8rixView()
        {
            // Менеджер создается внутри и привязывается к этому View
            _manager = new ForestM8rixManager(this);

            // Принудительно задаем начальные значения из DP
            _manager.Scale = this.Scale;
        }

        static ForestM8rixView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForestM8rixView),
                new FrameworkPropertyMetadata(typeof(ForestM8rixView)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

            // [TAG] FIX_VISIBILITY: Чтобы OnRender был виден, 
            // у ScrollViewer не должно быть контента, который его перекроет.
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += (s, e) => {
                    _manager.HandleScroll(_scrollViewer.VerticalOffset, _scrollViewer.HorizontalOffset);
                    InvalidateVisual();
                };
            }
        }
        //protected override void OnRender(DrawingContext dc)
        //{
        //    // 1. Заливаем всё красным, чтобы проверить "пробитие" сквозь прозрачный скроллер
        //    dc.DrawRectangle(Brushes.Red, null, new Rect(RenderSize));

        //    if (_manager == null || _manager.Count == 0) return;

        //    // 2. Рисуем дерево без всяких смещений (0, 0)
        //    // Если оно появится под шапкой - значит всё ок.
        //    _manager.Display.Render(dc, RenderSize);
        //}

        // [СУТЬ]
        // [СУТЬ] Локализация проблемы внутри ForestM8rixServiceDisplay
        // [TAG] FORCED_RENDER
        // [СУТЬ] Исправляем OnRender для работы с шапкой
        protected override void OnRender(DrawingContext dc)
        {
            // Оставляем синий фон (пока не настроим всё, он наш индикатор успеха)
            dc.DrawRectangle(Brushes.Blue, null, new Rect(RenderSize));

            if (_manager == null || _manager.Count == 0) return;

            // [TAG] FIX_COORDINATES: Сдвигаем всё дерево на 35 пикселей вниз
            dc.PushTransform(new TranslateTransform(0, HeaderHeight));

            // Рисуем данные. Передаем размер области БЕЗ шапки
            var dataSize = new Size(RenderSize.Width, Math.Max(0, RenderSize.Height - HeaderHeight));
            _manager.Display.Render(dc, dataSize);

            ExpandEverything();

            dc.Pop();
        }

        // [СУТЬ] Принудительное раскрытие при старте
        // [СУТЬ]
        public void ExpandEverything()
        {
            // 1. Если узлы — это просто объекты данных, 
            // состояние хранится в ExpansionService
            foreach (var node in _manager.Nodes)
            {
                // Используем сервис расширения, чтобы пометить узел как раскрытый
                _manager.Expansion.Toggle(node);
            }

            // 2. Просим менеджер перестроить плоский список из дерева
            _manager.Refresh();

            // 3. Перерисовываем синий экран
            InvalidateVisual();
        }

        //protected override void OnRender(DrawingContext dc)
        //{
        //    dc.DrawRectangle(Background ?? Brushes.White, null, new Rect(RenderSize));

        //    if (_manager == null || _manager.Count == 0) return;

        //    // [TAG] RENDERING_ZONE: Отрисовка строго под шапкой
        //    var dataArea = new Rect(0, HeaderHeight, RenderSize.Width, Math.Max(0, RenderSize.Height - HeaderHeight));

        //    dc.PushClip(new RectangleGeometry(dataArea));
        //    dc.PushTransform(new TranslateTransform(0, HeaderHeight));

        //    _manager.Display.Render(dc, dataArea.Size);

        //    dc.Pop(); // Pop Transform
        //    dc.Pop(); // Pop Clip
        //}

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (pos.Y >= HeaderHeight)
            {
                Point dataPos = new Point(pos.X, pos.Y - HeaderHeight);
                bool ctrl = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl);
                bool shift = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift);

                _manager.HandleMouseDown(dataPos, ctrl, shift);
            }
        }

        internal void UpdateExtent(Size extent)
        {
            if (_canvas != null)
            {
                _canvas.Width = extent.Width;
                _canvas.Height = extent.Height;
                InvalidateVisual();
            }
        }
    }
}