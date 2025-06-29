using System;

namespace Core
{
    public static class GameEvents
    {
        public static event Action<Guid, Guid> OnAttemptMatch;
        public static void RaiseAttemptMatch(Guid cell1, Guid cell2) => OnAttemptMatch?.Invoke(cell1, cell2);

        public static event Action OnUndoLastAction;
        public static void RaiseUndoLastAction() => OnUndoLastAction?.Invoke();

        public static event Action OnRequestNewGame;
        public static void RaiseRequestNewGame() => OnRequestNewGame?.Invoke();

        public static event Action OnAddExistingNumbers;
        public static void RaiseAddExistingNumbers() => OnAddExistingNumbers?.Invoke();

        public static event Action<bool> OnToggleTopLine;
        public static void RaiseToggleTopLine(bool isOn) => OnToggleTopLine?.Invoke(isOn);

        public static event Action<Guid, Guid> OnMatchFound;
        public static void RaiseMatchFound(Guid cell1, Guid cell2) => OnMatchFound?.Invoke(cell1, cell2);

        public static event Action OnInvalidMatch;
        public static void RaiseInvalidMatch() => OnInvalidMatch?.Invoke();

        public static event Action OnActionUndone;
        public static void RaiseActionUndone() => OnActionUndone?.Invoke();

        public static event Action OnGameStarted;
        public static void RaiseGameStarted() => OnGameStarted?.Invoke();

        public static event Action<bool> OnGridStateChanged;
        public static void RaiseGridStateChanged(bool isGridEmpty) => OnGridStateChanged?.Invoke(isGridEmpty);
    }
}