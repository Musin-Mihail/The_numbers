using Core.Events;
using UnityEngine;

namespace View.UI
{
    public class UIEventCaller : MonoBehaviour
    {
        [Header("Event Raising")]
        [SerializeField] private GameEvents gameEvents;

        public void RaiseRequestNewGame()
        {
            if (gameEvents) gameEvents.onRequestNewGame.Raise();
        }

        public void RaiseUndoLastAction()
        {
            if (gameEvents) gameEvents.onUndoLastAction.Raise();
        }

        public void RaiseAddExistingNumbers()
        {
            if (gameEvents) gameEvents.onAddExistingNumbers.Raise();
        }

        public void RaiseShowMenu()
        {
            if (gameEvents) gameEvents.onShowMenu.Raise();
        }

        public void RaiseHideMenu()
        {
            if (gameEvents) gameEvents.onHideMenu.Raise();
        }

        public void RaiseRequestHint()
        {
            if (gameEvents) gameEvents.onRequestHint.Raise();
        }

        public void RaiseRequestDisableCounters()
        {
            if (gameEvents) gameEvents.onRequestDisableCounters.Raise();
        }

        public void RaiseShowStatistics()
        {
            if (gameEvents) gameEvents.onShowStatistics.Raise();
        }

        public void RaiseHideStatistics()
        {
            if (gameEvents) gameEvents.onHideStatistics.Raise();
        }

        public void RaiseShowRules()
        {
            if (gameEvents) gameEvents.onShowRules.Raise();
        }

        public void RaiseHideRules()
        {
            if (gameEvents) gameEvents.onHideRules.Raise();
        }
    }
}