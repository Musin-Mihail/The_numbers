namespace Model
{
    public class ActionCountersModel
    {
        public int UndoCount { get; private set; }
        public int AddNumbersCount { get; private set; }
        public int HintCount { get; private set; }

        private const int InitialCount = 5;

        public ActionCountersModel()
        {
            ResetCounters();
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

        public bool IsUndoAvailable() => UndoCount > 0;
        public bool IsAddNumbersAvailable() => AddNumbersCount > 0;
        public bool IsHintAvailable() => HintCount > 0;
    }
}