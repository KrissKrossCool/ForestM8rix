using ForestM8rix.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ForestM8rix.Rendering;

// [ПОЛНЫЙ]
public class TemplateCellRenderer : ICellRenderer
{
    private readonly DataTemplate _template;
    private readonly FrameworkElement _view;

    public TemplateCellRenderer(DataTemplate template, FrameworkElement view)
    {
        _template = template;
        _view = view; // Сохраняем ссылку на ForestM8rixView
    }

    // [СУТЬ]
    public void Draw(DrawingContext dc, Rect rect, object node, bool isSelected)
    {
        // 1. Создаем виртуальный контейнер для шаблона
        var presenter = new ContentPresenter
        {
            Content = node,
            ContentTemplate = _template, // Хранится в поле класса
            Width = rect.Width,
            Height = rect.Height
        };

        // 2. Извлекаем статические настройки из контрола (View)
        // DependencyObject view — ссылка на основной контрол дерева
        Brush foreground = isSelected
            ? ForestM8rixOptions.GetSelectionForeground(_view)
            : ForestM8rixOptions.GetNormalForeground(_view);

        // [TAG] Магия: прокидываем цвет внутрь шаблона без изменения объекта node
        TextElement.SetForeground(presenter, foreground);

        // 3. Форсируем расчеты WPF
        presenter.Measure(rect.Size);
        presenter.Arrange(rect);

        // 4. "Снимок" в VisualBrush и отрисовка на полотно
        var vb = new VisualBrush(presenter) { Stretch = Stretch.None };
        dc.DrawRectangle(vb, null, rect);
    }
}
