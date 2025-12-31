//using ForestM8rix.StateManagement;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ForestM8rix;

//// Базовый интерфейс для всех действий
//public interface IForestCommand
//{
//    void Execute(ForestM8rixManager mgr);
//}


//#region Вспомогательные классы для системы команд

//public interface IForestCommand<out TResult>
//{
//    TResult Execute(ForestM8rixManager mgr);
//}

//public class ForestUpdateContext : IDisposable
//{
//    private readonly ForestM8rixManager _mgr;
//    public ForestUpdateContext(ForestM8rixManager mgr) { _mgr = mgr; _mgr.BeginUpdate(); }
//    public void Dispose() => _mgr.EndUpdate();
//}

//public class CommandExecutedEventArgs : EventArgs
//{
//    public string CommandName { get; }
//    public object Result { get; }
//    public CommandExecutedEventArgs(string name, object res) { CommandName = name; Result = res; }
//}

//// Пример базовой команды для интеграции
//public class ToggleExpandCommand : IForestCommand<bool>
//{
//    private readonly object _node;
//    public ToggleExpandCommand(object node) => _node = node;
//    public bool Execute(ForestM8rixManager mgr)
//    {
//        ForestStateRegistry.Toggle(_node);
//        return true;
//    }
//}

//#endregion