using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ForestM8rix;

public class ScrollData
{
    // Viewport - сколько строк влезает в окно
    public Size Viewport;
    // Extent - общее кол-во строк в BitArray
    public Size Extent;
    // Offset - текущий индекс верхней видимой строки
    public Vector Offset;

    public ScrollViewer ScrollOwner { get; set; }
}
