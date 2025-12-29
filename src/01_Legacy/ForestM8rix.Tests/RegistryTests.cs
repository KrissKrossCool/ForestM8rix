using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForestM8rix;
using System.Collections.Generic;

namespace ForestM8rix.Tests;

[TestClass]
public class RegistryTests
{
    public class MyNode
    {
        public uint ForestM8rixId { get; set; }
        public List<MyNode> Children { get; set; } = new();
    }

    [TestMethod]
    public void Process_Should_Handle_Deep_Tree_And_Levels()
    {
        // Arrange
        using var registry = new NodeRegistry();
        var root = new MyNode
        {
            Children = new List<MyNode> {
                new MyNode { Children = new List<MyNode> { new MyNode() } }
            }
        };
        var data = new List<MyNode> { root };

        // Act
        registry.Process(data, n => ((MyNode)n).Children);

        // Assert
        Assert.AreEqual(3, registry.Count, "Общее количество узлов не совпадает");

        // Проверка уровней: Root(0) -> Child(1) -> GrandChild(2)
        Assert.AreEqual(0, registry.Levels[0], "Неверный уровень корня");
        Assert.AreEqual(1, registry.Levels[1], "Неверный уровень первого потомка");
        Assert.AreEqual(2, registry.Levels[2], "Неверный уровень внука");

        // Проверка ID (сквозная нумерация)
        Assert.AreEqual(0u, root.ForestM8rixId);
        Assert.AreEqual(1u, root.Children[0].ForestM8rixId);
        Assert.AreEqual(2u, root.Children[0].Children[0].ForestM8rixId);
    }
}