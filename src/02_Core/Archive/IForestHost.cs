namespace ForestM8rix;

/// <summary>
/// Минимальный контракт, который View должен предоставить Менеджеру.
/// </summary>
public interface IForestHost
{
    // Свойства из FrameworkElement (подхватятся автоматически)
    double ActualHeight { get; }
    void InvalidateVisual();

    // Специфичные свойства нашего дерева
    double VerticalOffset { get; }
    double RowHeight { get; }
    double ViewportHeight { get; }
}