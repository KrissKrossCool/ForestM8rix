using System.Collections;
using System.Windows;

namespace ForestM8rix;

    public static class SelectionData
    {
        // === 1. OneItem (Single Selection) ===
        // Привязка: TwoWay по умолчанию
        public static readonly DependencyProperty OneItemProperty =
            DependencyProperty.RegisterAttached(
                "OneItem",
                typeof(object),
                typeof(SelectionData),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnOneItemChanged));

        public static object GetOneItem(DependencyObject obj) => obj.GetValue(OneItemProperty);
        public static void SetOneItem(DependencyObject obj, object value) => obj.SetValue(OneItemProperty, value);

        // === 2. List (Multi Selection) ===
        // Привязка: TwoWay по умолчанию
        public static readonly DependencyProperty ListProperty =
            DependencyProperty.RegisterAttached(
                "List",
                typeof(IList),
                typeof(SelectionData),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnListChanged));

        public static IList GetList(DependencyObject obj) => (IList)obj.GetValue(ListProperty);
        public static void SetList(DependencyObject obj, IList value) => obj.SetValue(ListProperty, value);


        // === ОБРАБОТЧИКИ ИЗМЕНЕНИЙ (ВХОДЯЩИЙ ПОТОК: VM -> VIEW) ===

        private static void OnOneItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Если свойство изменилось извне (из VM), сообщаем Менеджеру
            if (d is ForestM8rixView view && view.Manager != null)
            {
                // SetSelection сам разберется, это новый объект или тот же самый
                //view.Manager.SetSelection(e.NewValue);
            }
        }

        private static void OnListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Если список изменился (или заменили коллекцию)
            if (d is ForestM8rixView view && view.Manager != null)
            {
                //view.Manager.SetSelection(e.NewValue);
            }
        }
    }


