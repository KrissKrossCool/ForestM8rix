using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Tests.Core;

[TestClass]
public class RegistryLogicTests
{
    [TestMethod]
    public void Registry_Should_HandleBasicStateTransitions()
    {
        // 1. Создаем два разных объекта
        var nodeA = new object();
        var nodeB = new object();

        // 2. Устанавливаем разные состояния
        ForestStateRegistry.SetSelected(nodeA, true);
        ForestStateRegistry.SetExpanded(nodeA, false);

        ForestStateRegistry.SetSelected(nodeB, false);
        ForestStateRegistry.SetExpanded(nodeB, true);

        // 3. Проверяем точность попадания (валидация "клея" и "сот")
        Assert.IsTrue(ForestStateRegistry.IsSelected(nodeA), "NodeA должен быть выбран");
        Assert.IsFalse(ForestStateRegistry.IsExpanded(nodeA), "NodeA не должен быть развернут");

        Assert.IsFalse(ForestStateRegistry.IsSelected(nodeB), "NodeB не должен быть выбран");
        Assert.IsTrue(ForestStateRegistry.IsExpanded(nodeB), "NodeB должен быть развернут");
    }

    [TestMethod]
    public void Registry_Should_HandleSegmentOverrun()
    {
        // Проверяем, как работает переход через границу CELL_SIZE (64 бита)
        // Создаем 65 объектов, чтобы гарантированно зайти во вторую "соту" (BitArray)
        object lastNode = null;
        for (int i = 0; i < 65; i++)
        {
            lastNode = new object();
            ForestStateRegistry.SetSelected(lastNode, true);
        }

        Assert.IsTrue(ForestStateRegistry.IsSelected(lastNode), "65-й элемент (второй сегмент) должен быть выбран");
    }
}