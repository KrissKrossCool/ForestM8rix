using System.Windows;
using System.Windows.Media;

namespace ForestM8rix.Rendering;

/// <summary>
/// Контракт для отрисовки содержимого ячейки на DrawingContext.
/// Позволяет разделять логику отрисовки простого текста и сложных шаблонов.
/// </summary>
public interface ICellRenderer
{
    /// <summary>
    /// Отрисовывает содержимое ячейки.
    /// </summary>
    /// <param name="dc">Контекст рисования полотна.</param>
    /// <param name="rect">Область отрисовки (уже масштабированная под Scale).</param>
    /// <param name="node">Сырой объект данных пользователя.</param>
    /// <param name="isSelected">Флаг выделения, полученный из ForestStateRegistry.</param>
    void Draw(DrawingContext dc, Rect rect, object node, bool isSelected);
}