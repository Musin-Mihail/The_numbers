namespace Interfaces
{
    /// <summary>
    /// Интерфейс для действий, которые можно отменить.
    /// </summary>
    public interface IUndoableAction
    {
        /// <summary>
        /// Отменяет действие, восстанавливая предыдущее состояние.
        /// </summary>
        void Undo();
    }
}