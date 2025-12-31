using System.Windows;
using System.Windows.Media;

namespace ForestM8rix.Columns
{
    public class EmptyHeaderRenderer : IHeaderRenderer
    {
        public void Draw(DrawingContext dc, Rect rect)
        {
            // Метод пуст: если данных нет, мы просто ничего не рисуем. 
            // Это лучше, чем падение программы.
        }
    }
}