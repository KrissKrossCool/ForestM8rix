// [ПОЛНЫЙ]
using System;
using System.Windows;
using System.Windows.Media;
using ForestM8rix.Core;
using ForestM8rix.Columns;

namespace ForestM8rix.Rendering.CellRenderers
{
    public class TextCellRenderer : ICellRenderer
    {
        private readonly ForestM8rixManager _manager;
        private readonly ForestM8rixColumn _column;

        public TextCellRenderer(ForestM8rixManager manager, ForestM8rixColumn column)
        {
            _manager = manager;
            _column = column;
        }

        public void Draw(DrawingContext dc, Rect rect, object node, bool isSelected)
        {
            if (node == null || rect.Width <= 0) return;

            // 1. [TAG] Получение текста через кэшированную логику Вашей колонки
            string text = _column.GetText(node);
            if (string.IsNullOrEmpty(text)) return;

            // 2. [TAG] Подготовка кистей (цвета из опций)
            Brush textBrush = isSelected
                ? ForestM8rixOptions.GetSelectionForeground(_manager.View)
                : ForestM8rixOptions.GetNormalForeground(_manager.View);

            // 3. [TAG] Формирование текста с учетом масштаба
            // Используем стандартный Typeface из менеджера
            FormattedText ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _manager.DefaultTypeface ?? new Typeface("Segoe UI"),
                12 * _manager.Scale,
                textBrush,
                VisualTreeHelper.GetDpi(_manager.View).PixelsPerDip);

            // Настройка обрезки (Ellipsis)
            ft.MaxTextWidth = Math.Max(1, rect.Width - 10); // Запас на отступы
            ft.MaxTextHeight = rect.Height;
            ft.Trimming = TextTrimming.CharacterEllipsis;

            // Центрирование по вертикали
            double verticalOffset = (rect.Height - ft.Height) / 2;

            // Рисуем текст. Rect.X уже содержит в себе Indent, 
            // если это первая колонка (логика из DisplayService)
            dc.DrawText(ft, new Point(rect.X + 5, rect.Y + verticalOffset));
        }
    }
}