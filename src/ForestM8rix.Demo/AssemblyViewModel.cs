
//using ICSharpCode.TreeView;
using System.Collections.Generic;
using System.Linq;

namespace ForestM8rix.Demo;

public class AssemblyViewModel : SharpTreeNode//, IForestM8rixNode
{
    private bool _isVisible = true;
    private string _title; // Поле для хранения текста

    // Переопределяем Text для SharpTreeView
    public override object Text => _title;

    // Реализация интерфейса IForestM8rixNode
    //string IForestM8rixNode.DisplayName => _title;

    //IEnumerable<IForestM8rixNode>? Children => this.Children.Cast<IForestM8rixNode>();
    List<SharpTreeNode> Children => new();

    public new bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            RaisePropertyChanged(nameof(IsVisible));
        }
    }

    //bool IForestM8rixNode.IsExpanded
    //{
    //    get => this.IsExpanded;
    //    set => this.IsExpanded = value;
    //}

    public AssemblyViewModel(string title)
    {
        _title = title;
    }
}