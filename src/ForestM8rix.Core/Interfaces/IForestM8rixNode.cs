namespace ForestM8rix.Core.Interfaces
{
   // public interface IForestM8rixNode : ITreeTraversable<IForestM8rixNode>, IExpandableNode, IDisplayableNode, ISelectableNode { string Id { get; } }

    #region --- ЯДРО (Core) ---

    public interface ITreeTraversable<out T>
    {
        IEnumerable<T> Children { get; }
        T Parent { get; }
    }

    public interface IExpandableNode
    {
        bool IsExpanded { get; set; }
        bool HasChildren { get; set; }
        bool IsLoading { get; set; }
    }

    public interface IDisplayableNode
    {
        string Text { get; }            // Для системных нужд
        string DisplayName { get; }     // Для красивого UI
        object Icon { get; }
        string ToolTip { get; }
        int Level { get; }              // Уровень вложенности

        // Список действий для контекстного меню
        IEnumerable<object> ContextActions { get; }
    }

    public interface ISelectableNode
    {
        bool IsSelected { get; set; }
    }

    /// <summary>
    /// ГЛАВНЫЙ АГРЕГАТОР ЯДРА (Обязательный минимум)
    /// </summary>
    public interface IForestM8rixNode :
        ITreeTraversable<IForestM8rixNode>,
        IExpandableNode,
        IDisplayableNode,
        ISelectableNode
    {
        string Id { get; }
    }

    #endregion

    #region --- ОПЦИИ (Capabilities) ---

    public interface ICheckableNode { bool? IsChecked { get; set; } }
    public interface IFilterableNode { bool IsVisible { get; set; } }
    public interface ISearchableNode { bool IsMatched(string criteria); }

    #endregion
}
