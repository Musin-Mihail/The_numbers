using Core;
using UnityEngine;

namespace View.UI
{
    public class UIEventCaller : MonoBehaviour
    {
        public void RaiseRequestNewGame()
        {
            GameEvents.RaiseRequestNewGame();
        }

        public void RaiseUndoLastAction()
        {
            GameEvents.RaiseUndoLastAction();
        }

        public void RaiseAddExistingNumbers()
        {
            GameEvents.RaiseAddExistingNumbers();
        }

        public void RaiseShowMenu()
        {
            GameEvents.RaiseShowMenu();
        }

        public void RaiseHideMenu()
        {
            GameEvents.RaiseHideMenu();
        }
    }
}