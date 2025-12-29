using System;

namespace ForestM8rix;

public class ForestColumn
{
    public string Header { get; set; }
    public double Width { get; set; } = 150.0;

    // СУТЬ: Функция, которая говорит, ЧТО показывать в этой колонке
    public Func<object, string> CellTextSelector { get; set; }

    public ForestColumn(string header, double width, Func<object, string> selector)
    {
        Header = header;
        Width = width;
        CellTextSelector = selector;
    }
}