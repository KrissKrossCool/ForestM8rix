// [ПОЛНЫЙ]
using System;
using System.Collections.Generic;
using System.Linq;
using ForestM8rix.Columns;
using ForestM8rix.Core;

namespace ForestM8rix.Services
{
    public class ForestM8rixServiceColumn
    {
        private readonly ForestM8rixManager _mgr;
        private readonly List<IForestColumn> _columns = new();

        public IReadOnlyList<IForestColumn> All => _columns;

        // [СУТЬ] Динамический расчет ширины для UpdateExtent
        public double TotalWidth => _columns.Sum(c => c.Width);

        public ForestM8rixServiceColumn(ForestM8rixManager manager)
        {
            _mgr = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public void Add(IForestColumn column)
        {
            if (column == null) return;
            _columns.Add(column);

            // При добавлении колонки нужно расширить виртуальный холст
            _mgr.Display.UpdateMetrics();
        }

        public void Clear()
        {
            _columns.Clear();
            _mgr.Display.UpdateMetrics();
        }

        /// <summary>
        /// Позволяет найти колонку по индексу (например, для отрисовки в DisplayService)
        /// </summary>
        public IForestColumn GetAt(int index)
        {
            if (index < 0 || index >= _columns.Count) return null;
            return _columns[index];
        }
    }
}