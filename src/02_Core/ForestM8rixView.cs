// [ПОЛНЫЙ]
using ForestM8rix.Core;
using ForestM8rix.Rendering;
using ForestM8rix.StateManagement;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ForestM8rix
{
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Header", Type = typeof(ForestM8rixHeader))]
    public partial class ForestM8rixView : Control
    {
        private ScrollViewer _scrollViewer;
        private ForestCanvas _internalCanvas;
        private readonly ForestM8rixManager _manager;

        public ForestM8rixManager Manager => _manager;

        // Конструктор
        //public ForestM8rixView()
        //{
        //    // Передаем this, чтобы менеджер мог брать настройки (Font, DPI)
        //    _manager = new ForestM8rixManager(this);
        //}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

            // 1. [TAG] Настройка заголовка
            if (GetTemplateChild("PART_Header") is ForestM8rixHeader header)
            {
                // Привязываем колонки напрямую из нашего менеджера
                header.Columns = _manager.Columns;
                header.InvalidateVisual();
            }

            // 2. [TAG] Подмена холста
            if (GetTemplateChild("PART_Canvas") is Canvas placeholder)
            {
                _internalCanvas = new ForestCanvas
                {
                    OwnerManager = _manager,
                    Background = Brushes.Transparent // Критично для регистрации мыши
                };

                if (_scrollViewer != null)
                    _scrollViewer.Content = _internalCanvas;
            }

            // 3. [TAG] Синхронизация скролла
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += (s, e) =>
                {
                    // Обновляем оффсеты в менеджере для правильного клика
                    _manager.VerticalOffset = e.VerticalOffset / (_manager.RowHeight * _manager.Scale);
                    _manager.HorizontalOffset = e.HorizontalOffset;

                    // Синхронизируем заголовок (чтобы не уплывал при горизонтальном скролле)
                    if (GetTemplateChild("PART_Header") is ForestM8rixHeader h)
                    {
                        h.HorizontalOffset = e.HorizontalOffset;
                        h.InvalidateVisual();
                    }

                    _internalCanvas?.InvalidateVisual();
                };
            }

            if (_internalCanvas != null)
                _internalCanvas.MouseDown += OnCanvasMouseDown;
        }

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(_internalCanvas);
            double sRowH = _manager.RowHeight * _manager.Scale;

            // Индекс строки с учетом виртуализации/смещения
            int rowIdx = (int)(pos.Y / sRowH);

            if (rowIdx >= 0 && rowIdx < _manager.Nodes.Length)
            {
                object node = _manager.Nodes[rowIdx];
                int level = _manager.Levels[rowIdx];

                // Область экспандера (X)
                double indentX = level * _manager.IndentSize * _manager.Scale;
                double expanderClickArea = 20 * _manager.Scale;

                if (pos.X >= indentX && pos.X <= indentX + expanderClickArea)
                {
                    // Логика Toggle (Ваш подход)
                    bool isNowExpanded = !ForestStateRegistry.IsExpanded(node);
                    ForestStateRegistry.SetExpanded(node, isNowExpanded);

                    _manager.RefreshFlatList(); // Пересобираем дерево
                    UpdateLayout(); // Обновляем размеры ScrollViewer (ExtentHeight)
                }
                else
                {
                    // Обычный выбор строки
                    _manager.OnCanvasMouseDown(pos, Keyboard.Modifiers.HasFlag(ModifierKeys.Control), Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                }
            }
        }

        // [СУТЬ] Изолированный рендерер
        private class ForestCanvas : Canvas
        {
            public ForestM8rixManager OwnerManager { get; set; }

            protected override void OnRender(DrawingContext dc)
            {
                // Используем RenderManager для отрисовки всего дерева
                OwnerManager?.RenderAll(dc, RenderSize);
            }
        }
    }
}