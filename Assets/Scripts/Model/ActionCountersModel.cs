// --- Измененный файл: Assets/Scripts/Model/ActionCountersModel.cs ---

using Core;

namespace Model
{
    public class ActionCountersModel
    {
        public int UndoCount { get; private set; }
        public int AddNumbersCount { get; private set; }
        public int HintCount { get; private set; }
        public bool AreCountersDisabled { get; private set; }

        private const int InitialCount = Constants.InitialActionsCount;

        public ActionCountersModel()
        {
            ResetCounters();
        }

        public void RestoreState(ActionCountersModelSerializable data)
        {
            UndoCount = data.undoCount;
            AddNumbersCount = data.addNumbersCount;
            HintCount = data.hintCount;
            AreCountersDisabled = data.areCountersDisabled;
        }

        public void DecrementUndo()
        {
            if (UndoCount > 0) UndoCount--;
        }

        public void DecrementAddNumbers()
        {
            if (AddNumbersCount > 0) AddNumbersCount--;
        }

        public void DecrementHint()
        {
            if (HintCount > 0) HintCount--;
        }

        public void ResetCounters()
        {
            UndoCount = InitialCount;
            AddNumbersCount = InitialCount;
            HintCount = InitialCount;
        }

        public void DisableCounters()
        {
            AreCountersDisabled = true;
        }

        public bool IsUndoAvailable() => AreCountersDisabled || UndoCount > 0;
        public bool IsAddNumbersAvailable() => AreCountersDisabled || AddNumbersCount > 0;
        public bool IsHintAvailable() => AreCountersDisabled || HintCount > 0;
    }
}