using System.Collections.Generic;
using Interfaces;

namespace Core.UndoSystem
{
    /// <summary>
    /// Управляет историей действий, которые можно отменить.
    /// </summary>
    public class ActionHistory
    {
        private readonly Stack<IUndoableAction> _actions = new();

        /// <summary>
        /// Инициализирует новый экземпляр истории действий.
        /// </summary>
        public ActionHistory()
        {
        }

        /// <summary>
        /// Записывает новое действие в историю.
        /// </summary>
        /// <param name="action">Действие для записи.</param>
        public void Record(IUndoableAction action)
        {
            _actions.Push(action);
        }

        /// <summary>
        /// Отменяет последнее выполненное действие.
        /// </summary>
        public void Undo()
        {
            if (_actions.Count <= 0) return;
            var lastAction = _actions.Pop();
            lastAction.Undo();
        }

        /// <summary>
        /// Проверяет, возможно ли выполнить отмену.
        /// </summary>
        /// <returns>True, если есть действия для отмены.</returns>
        public bool CanUndo()
        {
            return _actions.Count > 0;
        }

        /// <summary>
        /// Очищает всю историю действий.
        /// </summary>
        public void Clear()
        {
            _actions.Clear();
        }
    }
}