using System.Collections.Generic;

namespace ForestM8rix.Testing
{
    // Базовый класс для всех узлов дерева
    public abstract class BaseNode
    {
        public string Title { get; set; }
        public List<BaseNode> Children { get; set; } = new List<BaseNode>();
    }

    // Класс папки (будет иметь иконку "Folder")
    public class FolderNode : BaseNode { }

    // Класс файла (будет иметь иконку "File" или "Node")
    public class FileNode : BaseNode
    {
        public long Size { get; set; }
    }

    public static class DemoDataGenerator
    {
        public static List<BaseNode> GetMockFilesystem()
        {
            var root = new FolderNode { Title = "Project_Forest" };

            var src = new FolderNode { Title = "Source" };
            src.Children.Add(new FileNode { Title = "ForestM8rixView.cs", Size = 12 });
            src.Children.Add(new FileNode { Title = "ForestManager.cs", Size = 45 });

            var docs = new FolderNode { Title = "Documentation" };
            docs.Children.Add(new FileNode { Title = "Readme.md", Size = 2 });

            root.Children.Add(src);
            root.Children.Add(docs);
            root.Children.Add(new FileNode { Title = "License.txt", Size = 1 });

            return new List<BaseNode> { root };
        }
    }
}