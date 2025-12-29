using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForestM8rix;
using System.Collections.Generic;

namespace ForestM8rix.Tests;

[TestClass]
public class ForestM8rixLogicTests
{
    [TestMethod]
    public void Viewport_Indices_Should_Be_Correct_At_Middle_Of_List()
    {
        // Arrange
        const double rowHeight = 25.0;
        const double viewHeight = 100.0; // Должно влезть 4-5 строк
        const double scrollOffset = 500.0; // Скролл на 20 строк вниз

        int totalNodes = 1000;

        // Act
        // Логика расчета (та же, что будет в OnRender)
        int start = (int)(scrollOffset / rowHeight);
        int end = (int)((scrollOffset + viewHeight) / rowHeight);

        // Assert
        Assert.AreEqual(20, start, "Должны начинать с 20-го индекса");
        Assert.AreEqual(24, end, "Должны закончить 24-м индексом");
        Assert.IsTrue(end < totalNodes);
    }
}