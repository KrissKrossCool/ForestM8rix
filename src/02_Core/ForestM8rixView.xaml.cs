// [ПОЛНЫЙ]
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ForestM8rix.Columns;
using ForestM8rix.Core;

namespace ForestM8rix;

public partial class ForestM8rixView : Control
{
    //private readonly ForestM8rixManager _manager;

    public ForestM8rixView()
    {
        // Инициализируем менеджера, передавая ему "себя" как хост для отрисовки
        _manager = new ForestM8rixManager(this);

        // Подписываемся на события мыши
        this.MouseDown += OnMouseDownInternal;

        // Улучшаем производительность: говорим WPF, что у нас есть фон
        this.Focusable = true;
    }

    private void OnMouseDownInternal(object sender, MouseButtonEventArgs e)
    {
        // Передаем управление менеджеру
        var pos = e.GetPosition(this);
        bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        _manager.OnCanvasMouseDown(pos, isCtrl, isShift);

        // Запрашиваем фокус для обработки клавиш в будущем
        this.Focus();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // Если данных нет — рисовать нечего
        if (_manager.Nodes == null || _manager.Nodes.Length == 0) return;

        double rowH = ForestM8rixOptions.GetRowHeight(this) * _manager.Scale;
        double viewW = this.ActualWidth;

        // В будущем здесь будет логика виртуализации (отрисовка только видимых строк)
        for (int i = 0; i < _manager.Nodes.Length; i++)
        {
            double y = i * rowH;
            // Временно передаем пустой список колонок или получаем их из свойств
            _manager.RenderRow(dc, i, y, rowH, viewW, GetCurrentColumns());
        }
    }

    private List<ForestM8rixColumn> GetCurrentColumns()
    {
        // Заглушка: в будущем здесь будет коллекция колонок из DP
        return new List<ForestM8rixColumn>();
    }

    // Обновляем параметры DPI при изменении монитора
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        _manager.UpdateVisualParams();
    }
}