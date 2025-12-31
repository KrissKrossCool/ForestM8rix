using System.Windows;
using System.Windows.Media;

namespace ForestM8rix.Columns;

public interface IHeaderRenderer
{
    void Draw(DrawingContext dc, Rect rect);
}

