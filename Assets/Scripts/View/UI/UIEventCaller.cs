using Core.Events;
using UnityEngine;

namespace View.UI
{
    public class UIEventCaller : MonoBehaviour
    {
        [Header("Event Raising")]
        [SerializeField] private VoidEvent onRequestNewGame;
        [SerializeField] private VoidEvent onUndoLastAction;
        [SerializeField] private VoidEvent onAddExistingNumbers;
        [SerializeField] private VoidEvent onShowMenu;
        [SerializeField] private VoidEvent onHideMenu;
        [SerializeField] private VoidEvent onRequestHint;
        [SerializeField] private VoidEvent onRequestDisableCounters;
        [SerializeField] private VoidEvent onShowStatistics;
        [SerializeField] private VoidEvent onHideStatistics;

        public void RaiseRequestNewGame()
        {
            if (onRequestNewGame) onRequestNewGame.Raise();
        }

        public void RaiseUndoLastAction()
        {
            if (onUndoLastAction) onUndoLastAction.Raise();
        }

        public void RaiseAddExistingNumbers()
        {
            if (onAddExistingNumbers) onAddExistingNumbers.Raise();
        }

        public void RaiseShowMenu()
        {
            if (onShowMenu) onShowMenu.Raise();
        }

        public void RaiseHideMenu()
        {
            if (onHideMenu) onHideMenu.Raise();
        }

        public void RaiseRequestHint()
        {
            if (onRequestHint) onRequestHint.Raise();
        }

        public void RaiseRequestDisableCounters()
        {
            if (onRequestDisableCounters) onRequestDisableCounters.Raise();
        }

        public void RaiseShowStatistics()
        {
            if (onShowStatistics) onShowStatistics.Raise();
        }

        public void RaiseHideStatistics()
        {
            if (onHideStatistics) onHideStatistics.Raise();
        }
    }
}