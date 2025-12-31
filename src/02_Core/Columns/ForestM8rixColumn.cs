using System.Windows;

namespace ForestM8rix.Columns
{
    public class ForestM8rixColumn : DependencyObject
    {
        // Поля для оптимизации (кэширования)
        private IHeaderRenderer _cachedRenderer;
        private DataTemplate _lastTemplate;

        // Основные свойства (POCO для MVP)
        public string Title { get; set; }
        public double Width { get; set; } = 100;
        public DataTemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Возвращает инструмент для отрисовки шапки. 
        /// Реализует логику кэширования: создает новый объект только при смене шаблона.
        /// </summary>
        public IHeaderRenderer GetHeaderRenderer()
        {
            // Если шаблон подменили в процессе работы
            if (_lastTemplate != HeaderTemplate)
            {
                _cachedRenderer = null;
                _lastTemplate = HeaderTemplate;
            }

            // Создаем рендерер один раз
            if (_cachedRenderer == null)
            {
                _cachedRenderer = HeaderTemplate != null
                    ? new TemplateHeaderRenderer(this)
                    : new TextHeaderRenderer(this);
            }

            return _cachedRenderer;
        }
    }
}