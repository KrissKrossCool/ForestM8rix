using System.Windows;
using System.Windows.Media;
using ForestM8rix.Core;

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
            if (d is ForestM8rixView view && view.Manager != null)
            {
                // Пробрасываем значение прямо в менеджер
                // Обратите внимание: код View вообще не знает об этом свойстве!
               // view.Manager.IsRubberBandEnabled = (bool)e.NewValue;
            }
        }

        // [СУТЬ]
        public static readonly DependencyProperty RowHeightProperty =
            DependencyProperty.RegisterAttached(
                "RowHeight",
                typeof(double),
                typeof(ForestM8rixOptions),
                new PropertyMetadata(24.0)); // Значение по умолчанию

        public static double GetRowHeight(DependencyObject obj) => (double)obj.GetValue(RowHeightProperty);
        public static void SetRowHeight(DependencyObject obj, double value) => obj.SetValue(RowHeightProperty, value);

        // [NEW] Фон обычной (не выделенной) строки
        public static readonly DependencyProperty NormalBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "NormalBackground",
                typeof(Brush),
                typeof(ForestM8rixOptions),
                new PropertyMetadata(Brushes.Transparent));

        // [NEW] Фон выделенной строки
        public static readonly DependencyProperty SelectionBackgroundProperty =
            DependencyProperty.RegisterAttached("SelectionBackground", typeof(Brush), typeof(ForestM8rixOptions), new PropertyMetadata(Brushes.RoyalBlue));

        // [NEW] Цвет текста выделенной строки
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.RegisterAttached("SelectionForeground", typeof(Brush), typeof(ForestM8rixOptions), new PropertyMetadata(Brushes.White));

        // [NEW] Цвет текста обычной строки
        public static readonly DependencyProperty NormalForegroundProperty =
            DependencyProperty.RegisterAttached("NormalForeground", typeof(Brush), typeof(ForestM8rixOptions), new PropertyMetadata(Brushes.Black));

        public static Brush GetNormalBackground(DependencyObject obj) => (Brush)obj.GetValue(NormalBackgroundProperty);
        public static void SetNormalBackground(DependencyObject obj, Brush value) => obj.SetValue(NormalBackgroundProperty, value);

        public static Brush GetSelectionBackground(DependencyObject obj) => (Brush)obj.GetValue(SelectionBackgroundProperty);
        public static void SetSelectionBackground(DependencyObject obj, Brush value) => obj.SetValue(SelectionBackgroundProperty, value);

        public static Brush GetSelectionForeground(DependencyObject obj) => (Brush)obj.GetValue(SelectionForegroundProperty);
        public static void SetSelectionForeground(DependencyObject obj, Brush value) => obj.SetValue(SelectionForegroundProperty, value);

        public static Brush GetNormalForeground(DependencyObject obj) => (Brush)obj.GetValue(NormalForegroundProperty);
        public static void SetNormalForeground(DependencyObject obj, Brush value) => obj.SetValue(NormalForegroundProperty, value);
    }
}
