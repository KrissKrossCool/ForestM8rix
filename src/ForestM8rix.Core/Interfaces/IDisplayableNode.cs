namespace ForestM8rix.Core.Interfaces;

public interface IDisplayableNode { 
string Text { get; }
string DisplayName { get; } 
object Icon { get; } 
string ToolTip { get; } 
int Level { get; } 
}
