using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ForestM8rix.Core;
using ForestM8rix.Rendering;
using ForestM8rix.StateManagement;

namespace ForestM8rix.Services
{
    public class ForestM8rixServiceDisplay
    {
        private readonly ForestM8rixManager _mgr;

        public double RowHeight { get; set; } = 25.0;
        public double VerticalOffset { get; set; }   // Индекс первой видимой строки
        public double HorizontalOffset { get; set; } // Смещение в пикселях

        public ForestM8rixServiceDisplay(ForestM8rixManager manager)
        {
            _mgr = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        private double ScaledRowHeight => RowHeight * _mgr.Scale;

        public void UpdateMetrics()
        {
            double totalHeight = _mgr.Count * ScaledRowHeight;
            double totalWidth = _mgr.Column.TotalWidth * _mgr.Scale;

            if (_mgr.View is ForestM8rixView hostView)
            {
                hostView.UpdateExtent(new Size(totalWidth, totalHeight));
            }
        }

        public void Render(DrawingContext dc, Size viewportSize)
        {
            if (_mgr.Count == 0) return;

            double sRowH = ScaledRowHeight;
            int startIdx = (int)Math.Max(0, VerticalOffset);
            int visibleCount = (int)Math.Ceiling(viewportSize.Height / sRowH) + 1;
            int endIdx = Math.Min(_mgr.Count, startIdx + visibleCount);

            for (int i = startIdx; i < endIdx; i++)
            {
                // Отрисовка в координатах Canvas (от 0)
                double y = (i - startIdx) * sRowH;
                RenderRow(dc, i, y, sRowH, viewportSize.Width);
            }
        }

        private void RenderRow(DrawingContext dc, int idx, double y, double rowH, double viewW)
        {
            if (idx >= _mgr.Nodes.Length) return;
            object node = _mgr.Nodes[idx];
            bool isSelected = ForestStateRegistry.IsSelected(node);

            if (isSelected)
            {
                Brush bg = ForestM8rixOptions.GetSelectionBackground(_mgr.View);
                dc.DrawRectangle(bg, null, new Rect(0, y, viewW, rowH));
            }

            // Отрисовка колонок
            double currentX = -HorizontalOffset;
            var columns = _mgr.Column.All;

            for (int i = 0; i < columns.Count; i++)
            {
                double colW = columns[i].Width * _mgr.Scale;
                if (currentX + colW > 0 && currentX < viewW)
                {
                    var cellRect = new Rect(currentX, y, colW, rowH);
                    if (i == 0) RenderTreeCell(dc, cellRect, idx, node, isSelected);
                    else columns[i].GetCellRenderer(_mgr).Draw(dc, cellRect, node, isSelected);
                }
                currentX += colW;
            }
        }

        private void RenderTreeCell(DrawingContext dc, Rect rect, int idx, object node, bool isSelected)
        {
            int level = _mgr.Levels[idx];
            double sIndent = _mgr.IndentSize * _mgr.Scale;
            double x = rect.Left;

            ForestRenderHelper.DrawTreeLines(dc, level, x, rect.Top, rect.Height, sIndent,
                _mgr.IsLastChild(idx), _mgr.GetParentHierarchyInfo(idx));

            if (_mgr.HasChildren(node))
            {
                // Центр зоны отступа для экспандера
                var expCenter = new Point(x + (level * sIndent) + (sIndent / 2), rect.Top + rect.Height / 2);
                ForestRenderHelper.DrawExpander(dc, expCenter, ForestStateRegistry.IsExpanded(node), Brushes.Gray);
            }

            // Текст колонки с учетом увеличенного экспандера
            double contentOffset = (level * sIndent) + (sIndent * 1.3);
            var contentRect = new Rect(x + contentOffset, rect.Top, rect.Width - contentOffset, rect.Height);
            _mgr.Column.All[0].GetCellRenderer(_mgr).Draw(dc, contentRect, node, isSelected);
        }

        public int GetIndexFromY(double y)
        {
            if (ScaledRowHeight <= 0) return -1;
            int idx = (int)(y / ScaledRowHeight) + (int)VerticalOffset;
            return (idx >= 0 && idx < _mgr.Nodes.Length) ? idx : -1;
        }

        public void EnsureVisible(int idx)
        {
            // Если целевой индекс выше текущей видимой области
            if (idx < VerticalOffset)
            {
                VerticalOffset = idx;
            }
            // Здесь позже добавим логику для нижней границы, 
            // когда внедрим полноценный IScrollInfo
        }

        public bool IsExpanderHit(int idx, double x)
        {
            if (idx < 0 || idx >= _mgr.Levels.Length) return false;

            int level = _mgr.Levels[idx];
            double sIndent = _mgr.IndentSize * _mgr.Scale;

            // Границы зоны клика
            double expanderLeft = (level * sIndent) - HorizontalOffset;
            double expanderRight = expanderLeft + sIndent;

            bool isHit = x >= expanderLeft && x <= expanderRight;

            Debug.WriteLine($"[HIT] Idx:{idx} MouseX:{x:F1} Range:{expanderLeft:F1}-{expanderRight:F1} Result:{isHit}");
            return isHit;
        }
    }
}