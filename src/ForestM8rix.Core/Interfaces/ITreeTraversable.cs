namespace ForestM8rix.Core.Interfaces
{
    public interface ITreeTraversable<out T> { IEnumerable<T> Children { get; } T Parent { get; } }
}
