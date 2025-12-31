using System.Windows;

namespace ForestM8rix.Columns
{
    public class ForestM8rixColumn : DependencyObject
    {
        private IHeaderRenderer? _cachedRenderer;
        private DataTemplate? _lastAppliedTemplate;
        private string? _lastTitle;

        // [СУТЬ] DependencyProperty для Width (чтобы работали триггеры)
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(nameof(Width), typeof(double), typeof(ForestM8rixColumn),
                new PropertyMetadata(100.0));

        public double Width
        {
            get => (double)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        // [СУТЬ] DependencyProperty для HeaderTemplate
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ForestM8rixColumn),
                new PropertyMetadata(null));

        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public string? Title { get; set; } // Можно оставить обычным, если не биндим его

        /// <summary>
        /// Возвращает рендерер. Если XAML-стиль подменил HeaderTemplate через триггер,
        /// этот метод мгновенно пересоздаст нужный рендерер.
        /// </summary>
        public IHeaderRenderer GetHeaderRenderer()
        {
            var activeTemplate = HeaderTemplate;

            if (_cachedRenderer == null || _lastAppliedTemplate != activeTemplate || _lastTitle != Title)
            {
                _lastAppliedTemplate = activeTemplate;
                _lastTitle = Title;

                if (activeTemplate != null)
                    _cachedRenderer = new TemplateHeaderRenderer(this);
                else if (!string.IsNullOrEmpty(Title))
                    _cachedRenderer = new TextHeaderRenderer(this);
                else
                    _cachedRenderer = new EmptyHeaderRenderer();
            }

            return _cachedRenderer;
        }
    }
}