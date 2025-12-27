namespace ForestM8rix.Core.Interfaces
{
    public interface IForestM8rixNode
    {
        string DisplayName { get; }
        System.Collections.Generic.IEnumerable<IForestM8rixNode>? Children { get; }
        bool IsVisible { get; set; }
        bool IsExpanded { get; set; }
    }
}
