//using ForestM8rix.Core;
//using ForestM8rix.Rendering; // [TAG] Здесь должны лежать Ваши рендереры
//using ForestM8rix.Rendering.CellRenderers;
//using System;
//using System.Reflection;
//using System.Windows;

//namespace ForestM8rix.Columns
//{
//    public class ForestM8rixColumn
//    {
//        public string Header { get; set; }
//        public double Width { get; set; } = 150;

//        // Шаблоны для кастомизации
//        public DataTemplate HeaderTemplate { get; set; }
//        public DataTemplate CellTemplate { get; set; }

//        // Кэшированные рендереры, чтобы не создавать их каждый кадр
//        private ICellRenderer _cellRenderer;
//        private IHeaderRenderer _headerRenderer;

//        // Привязки данных
//        public string BindingPath { get; set; }
//        public Func<object, string> CellTextSelector { get; set; }

//        // Иконки
//        public string IconKey { get; set; } = "Node";
//        public Func<object, string> IconSelector { get; set; }

//        private PropertyInfo _boundProperty;
//        private Type _lastType;

//        /// <summary>
//        /// [TAG] Метод выбора рендерера для шапки
//        /// Устраняет ошибку в ForestM8rixHeader
//        /// </summary>
//        public IHeaderRenderer GetHeaderRenderer(FrameworkElement host)
//        {
//            if (HeaderTemplate != null)
//            {
//                // Если есть шаблон, используем сложный рендерер
//                return new TemplateHeaderRenderer(HeaderTemplate, host, this);
//            }

//            // По умолчанию - текстовый рендерер
//            return new TextHeaderRenderer(host, this);
//        }

//        public string GetText(object item)
//        {
//            if (item == null) return string.Empty;
//            if (CellTextSelector != null) return CellTextSelector(item);

//            if (!string.IsNullOrEmpty(BindingPath))
//            {
//                var type = item.GetType();
//                if (type != _lastType)
//                {
//                    _boundProperty = type.GetProperty(BindingPath);
//                    _lastType = type;
//                }
//                return _boundProperty?.GetValue(item)?.ToString() ?? string.Empty;
//            }
//            return item.ToString();
//        }

//        // [TAG] Метод получения рендерера для ячеек
//        public ICellRenderer GetCellRenderer(ForestM8rixManager manager)
//        {
//            if (_cellRenderer != null) return _cellRenderer;

//            // Если пользователь задал DataTemplate — используем наш новый TemplateCellRenderer
//            if (CellTemplate != null)
//            {
//                _cellRenderer = new TemplateCellRenderer(CellTemplate, manager.View);
//            }
//            else
//            {
//                // Иначе — стандартный текстовый рендерер (нужно создать этот класс аналогично)
//                _cellRenderer = new TextCellRenderer(manager);
//            }

//            return _cellRenderer;
//        }

//        // Аналогично для шапки (помним про передачу 'this' в конструктор по Вашему правилу)
//        public IHeaderRenderer GetHeaderRenderer(ForestM8rixHeader headerControl)
//        {
//            if (_headerRenderer != null) return _headerRenderer;

//            if (HeaderTemplate != null)
//                _headerRenderer = new TemplateHeaderRenderer(HeaderTemplate, headerControl, this);
//            else
//                _headerRenderer = new TextHeaderRenderer(headerControl, this);

//            return _headerRenderer;
//        }

//        public string GetIconKey(object item) => IconSelector?.Invoke(item) ?? IconKey;
//    }
//}


// [ПОЛНЫЙ]
using ForestM8rix.Core;
using ForestM8rix.Rendering;
using ForestM8rix.Rendering.CellRenderers;
using System;
using System.Reflection;
using System.Windows;

namespace ForestM8rix.Columns;

    public class ForestM8rixColumn : IForestColumn // Подключаем интерфейс для менеджера
    {
        public string Header { get; set; }
        public double Width { get; set; } = 150;

        public DataTemplate HeaderTemplate { get; set; }
        public DataTemplate CellTemplate { get; set; }

        private ICellRenderer _cellRenderer;
        private IHeaderRenderer _headerRenderer;

        public string BindingPath { get; set; }
        public Func<object, string> CellTextSelector { get; set; }

        public string IconKey { get; set; } = "Node";
        public Func<object, string> IconSelector { get; set; }

        private PropertyInfo _boundProperty;
        private Type _lastType;

    // [TAG] Реализация интерфейса IForestColumn
    // [СУТЬ]
    public ICellRenderer GetCellRenderer(ForestM8rixManager manager)
    {
        if (_cellRenderer != null) return _cellRenderer;

        if (CellTemplate != null)
            _cellRenderer = new TemplateCellRenderer(CellTemplate, manager.View);
        else
            // ПЕРЕДАЕМ this (колонку) по правилу
            _cellRenderer = new TextCellRenderer(manager, this);

        return _cellRenderer;
    }

    /// <summary>
    /// [TAG] Метод выбора рендерера для шапки.
    /// СОГЛАСНО ПРАВИЛУ: передаем 'this' в конструктор.
    /// </summary>
    public IHeaderRenderer GetHeaderRenderer(FrameworkElement host)
        {
            // Не кэшируем здесь, если host может меняться, 
            // либо используем перегрузку ниже для ForestM8rixHeader
            if (HeaderTemplate != null)
            {
                return new TemplateHeaderRenderer(HeaderTemplate, host, this);
            }

            return new TextHeaderRenderer(host, this);
        }

        // [TAG] Перегрузка для конкретного контрола заголовка
        public IHeaderRenderer GetHeaderRenderer(ForestM8rixHeader headerControl)
        {
            if (_headerRenderer != null) return _headerRenderer;

            if (HeaderTemplate != null)
                _headerRenderer = new TemplateHeaderRenderer(HeaderTemplate, headerControl, this);
            else
                _headerRenderer = new TextHeaderRenderer(headerControl, this);

            return _headerRenderer;
        }

        public string GetText(object item)
        {
            if (item == null) return string.Empty;
            if (CellTextSelector != null) return CellTextSelector(item);

            if (!string.IsNullOrEmpty(BindingPath))
            {
                var type = item.GetType();
                if (type != _lastType)
                {
                    _boundProperty = type.GetProperty(BindingPath);
                    _lastType = type;
                }
                return _boundProperty?.GetValue(item)?.ToString() ?? string.Empty;
            }
            return item.ToString();
        }

        public string GetIconKey(object item) => IconSelector?.Invoke(item) ?? IconKey;
    }


public interface IForestColumn
{
    string Header { get; set; }
    double Width { get; set; }
    string BindingPath { get; set; }
    ICellRenderer GetCellRenderer(ForestM8rixManager manager);
}