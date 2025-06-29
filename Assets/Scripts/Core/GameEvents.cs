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

        public static event Action OnGameStarted;
        public static void RaiseGameStarted() => OnGameStarted?.Invoke();

        public static event Action OnShowMenu;
        public static void RaiseShowMenu() => OnShowMenu?.Invoke();

        public static event Action OnHideMenu;
        public static void RaiseHideMenu() => OnHideMenu?.Invoke();

        public static event Action OnRequestHint;
        public static void RaiseRequestHint() => OnRequestHint?.Invoke();

        public static event Action<Guid, Guid> OnHintFound;
        public static void RaiseHintFound(Guid cell1, Guid cell2) => OnHintFound?.Invoke(cell1, cell2);

        public static event Action OnNoHintFound;
        public static void RaiseNoHintFound() => OnNoHintFound?.Invoke();

        public static event Action OnClearHint;
        public static void RaiseClearHint() => OnClearHint?.Invoke();

        public static event Action<int, int, int> OnCountersChanged;
        public static void RaiseCountersChanged(int undo, int add, int hint) => OnCountersChanged?.Invoke(undo, add, hint);

        public static event Action OnRequestRefillCounters;
        public static void RaiseRequestRefillCounters() => OnRequestRefillCounters?.Invoke();

        public static event Action OnRefillCountersConfirmed;
        public static void RaiseRefillCountersConfirmed() => OnRefillCountersConfirmed?.Invoke();

        public static event Action OnRequestDisableCounters;
        public static void RaiseRequestDisableCounters() => OnRequestDisableCounters?.Invoke();

        public static event Action OnDisableCountersConfirmed;
        public static void RaiseDisableCountersConfirmed() => OnDisableCountersConfirmed?.Invoke();
    }
}