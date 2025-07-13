using Core;

namespace Model
{
    /// <summary>
    /// Модель данных для хранения количества доступных действий игрока (отмена, добавление, подсказка).
    /// </summary>
    public class ActionCountersModel
    {
        /// <summary>
        /// Количество доступных отмен хода.
        /// </summary>
        public int UndoCount { get; private set; }

        /// <summary>
        /// Количество доступных добавлений чисел.
        /// </summary>
        public int AddNumbersCount { get; private set; }

        /// <summary>
        /// Количество доступных подсказок.
        /// </summary>
        public int HintCount { get; private set; }

        /// <summary>
        /// Указывает, отключены ли счетчики (например, после покупки).
        /// </summary>
        public bool AreCountersDisabled { get; private set; }

        private const int InitialCount = Constants.InitialActionsCount;

        public ActionCountersModel()
        {
            ResetCounters();
        }

        /// <summary>
        /// Восстанавливает состояние счетчиков из сохраненных данных.
        /// </summary>
        /// <param name="data">Сериализованные данные счетчиков.</param>
        public void RestoreState(ActionCountersModelSerializable data)
        {
            UndoCount = data.undoCount;
            AddNumbersCount = data.addNumbersCount;
            HintCount = data.hintCount;
            AreCountersDisabled = data.areCountersDisabled;
        }

        /// <summary>
        /// Уменьшает счетчик отмен на единицу.
        /// </summary>
        public void DecrementUndo()
        {
            if (UndoCount > 0) UndoCount--;
        }

        /// <summary>
        /// Уменьшает счетчик добавлений чисел на единицу.
        /// </summary>
        public void DecrementAddNumbers()
        {
            if (AddNumbersCount > 0) AddNumbersCount--;
        }

        /// <summary>
        /// Уменьшает счетчик подсказок на единицу.
        /// </summary>
        public void DecrementHint()
        {
            if (HintCount > 0) HintCount--;
        }

        /// <summary>
        /// Сбрасывает все счетчики к начальным значениям.
        /// </summary>
        public void ResetCounters()
        {
            UndoCount = InitialCount;
            AddNumbersCount = InitialCount;
            HintCount = InitialCount;
        }

        /// <summary>
        /// Отключает ограничения на количество действий.
        /// </summary>
        public void DisableCounters()
        {
            AreCountersDisabled = true;
        }

        /// <summary>
        /// Проверяет, доступна ли отмена хода.
        /// </summary>
        public bool IsUndoAvailable() => AreCountersDisabled || UndoCount > 0;

        /// <summary>
        /// Проверяет, доступно ли добавление чисел.
        /// </summary>
        public bool IsAddNumbersAvailable() => AreCountersDisabled || AddNumbersCount > 0;

        /// <summary>
        /// Проверяет, доступна ли подсказка.
        /// </summary>
        public bool IsHintAvailable() => AreCountersDisabled || HintCount > 0;
    }
}