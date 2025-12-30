using System.Windows;

namespace ForestM8rix
{
    // Отдельный статический класс для настроек
    public static class ForestM8rixOptions
    {
        // 1. Регистрируем Attached Property
        public static readonly DependencyProperty IsRubberBandEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsRubberBandEnabled",
                typeof(bool),
                typeof(ForestM8rixOptions),
                new PropertyMetadata(true, OnRubberBandChanged));

        // Стандартные геттеры/сеттеры для XAML
        public static void SetIsRubberBandEnabled(DependencyObject element, bool value)
            => element.SetValue(IsRubberBandEnabledProperty, value);

        public static bool GetIsRubberBandEnabled(DependencyObject element)
            => (bool)element.GetValue(IsRubberBandEnabledProperty);

        // 2. Магия обновления: метод срабатывает, когда в XAML меняют свойство
        private static void OnRubberBandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Проверяем, что свойство повесили именно на наш контрол
            if (d is ForestM8rixView view)
            {
                // Пробрасываем значение прямо в менеджер
                // Обратите внимание: код View вообще не знает об этом свойстве!
                view.Manager.IsRubberBandEnabled = (bool)e.NewValue;
            }
        }
    }
}
