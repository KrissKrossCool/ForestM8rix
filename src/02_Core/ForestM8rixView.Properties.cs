using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ForestM8rix
{
    // [ПОЛНЫЙ] Часть 1: Свойства зависимости
    public partial class ForestM8rixView : Control
    {
        static ForestM8rixView()
        {
            // Сообщаем WPF, что мы сами рисуем контрол (OnRender), а не через шаблон
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForestM8rixView),
                new FrameworkPropertyMetadata(typeof(ForestM8rixView)));
        }

        // Данные (Дерево)
        public static readonly DependencyProperty ForestInputProperty =
            DependencyProperty.Register(nameof(ForestInput), typeof(IEnumerable), typeof(ForestM8rixView),
                new PropertyMetadata(null, OnForestInputChanged));

        public IEnumerable ForestInput
        {
            get => (IEnumerable)GetValue(ForestInputProperty);
            set => SetValue(ForestInputProperty, value);
        }

        // Логика детей
        public static readonly DependencyProperty ChildSelectorProperty =
            DependencyProperty.Register(nameof(ChildSelector), typeof(Func<object, IEnumerable>), typeof(ForestM8rixView),
                new PropertyMetadata(null, OnChildSelectorChanged));

        public Func<object, IEnumerable> ChildSelector
        {
            get => (Func<object, IEnumerable>)GetValue(ChildSelectorProperty);
            set => SetValue(ChildSelectorProperty, value);
        }

        // Визуал: Масштаб
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(ForestM8rixView),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualParamChanged));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        // [СУТЬ]
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(nameof(HeaderHeight), typeof(double), typeof(ForestM8rixView),
                new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        private static void OnForestInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((ForestM8rixView)d)._manager?.SetItemsSource(e.NewValue as IEnumerable, ((ForestM8rixView)d).ChildSelector);

        private static void OnChildSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ForestM8rixView)d;
            if (view._manager != null)
            {
                view._manager.ChildSelector = e.NewValue as Func<object, IEnumerable>;
                view._manager.RefreshFlatList();
            }
        }

        private static void OnVisualParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ForestM8rixView)d;
            if (view._manager != null)
            {
                view._manager.Scale = view.Scale;
                view.InvalidateVisual();
            }
        }
    }
}