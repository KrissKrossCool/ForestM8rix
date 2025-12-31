// [СУТЬ] 1. Холст для отрисовки строк
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ForestM8rix;
public class ForestM8rixCanvas : FrameworkElement
{
    // Пока пустой, чтобы XAML его "увидел"
    protected override void OnRender(DrawingContext dc)
    {
        // Сюда придет отрисовка строк из Менеджера
    }
}

// [СУТЬ] 2. Шапка для заголовков колонок
//public class ForestM8rixHeader : FrameworkElement
//{
//    protected override void OnRender(DrawingContext dc)
//    {
//        // Сюда придет отрисовка заголовков
//    }
//}

