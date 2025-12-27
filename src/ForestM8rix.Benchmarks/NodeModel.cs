using ForestM8rix.Core.Interfaces;
using System.Collections.Generic;

namespace ForestM8rix.Benchmarks
{
    public class NodeModel : IForestM8rixNode
    {
        public string DisplayName { get; init; } = string.Empty;
        public IEnumerable<IForestM8rixNode>? Children { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsExpanded { get; set; }
    }
}