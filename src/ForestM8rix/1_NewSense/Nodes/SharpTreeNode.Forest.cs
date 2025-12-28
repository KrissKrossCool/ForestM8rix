using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ForestM8rix.StateManagement;

namespace ForestM8rix
{
    /// <summary>
    /// [TAG] Расширение SharpTreeNode для проекта ForestM8rix.
    /// Содержит только необходимые дополнения к оригинальному классу.
    /// </summary>
    public partial class SharpTreeNode : INotifyPropertyChanged
    {
        #region [TAG] Данные

        //public int Id { get; set; }

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => Set(ref _displayName, value);
        }

        private object? _icon;
        public  object? Icon
        {
            get => _icon;
            set => Set(ref _icon, value);
        }

        private object? _toolTip;
        public  object? ToolTip
        {
            get => _toolTip;
            set => Set(ref _toolTip, value);
        }

        #endregion

        #region [TAG] Состояния (Registry)

        /// <summary>
        /// Выделение узла. Данные управляются ForestStateRegistry.
        /// </summary>
        public bool IsSelected
        {
            get => ForestStateRegistry.IsSelected(this);
            set
            {
                if (ForestStateRegistry.IsSelected(this) != value)
                {
                    ForestStateRegistry.SetSelected(this, value);

                    // Временный дебаг из прокси можно оставить здесь для тестов
                    System.Diagnostics.Debug.WriteLine($"[Registry] Node {DisplayName} Selected: {value}");

                    Notify();
                }
            }
        }

        #endregion

        #region [TAG] Инфраструктура уведомлений

        //public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string prop = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            Notify(prop);
            return true;
        }

        public void Notify([CallerMemberName] string prop = "")
        {
            // 1. Вызываем метод из оригинальных 730 строк для синхронизации библиотеки
            RaisePropertyChanged(prop);

            // 2. Стандартное событие для WPF Binding
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion
    }
}