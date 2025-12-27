namespace ForestM8rix.Core.Interfaces
{
    public interface IExpandableNode { bool IsExpanded { get; set; } bool HasChildren { get; set; } bool IsLoading { get; set; } }
}
