using Core.Events;
using UnityEngine;

namespace View.UI
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowMenu;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (!gameEvents) return;
            gameEvents.onShowMenu.AddListener(ShowMenu);
            gameEvents.onHideMenu.AddListener(HideMenu);
            gameEvents.onNewGameStarted.AddListener(HideMenu);
        }

        private void Start()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (!gameEvents) return;
            gameEvents.onShowMenu.RemoveListener(ShowMenu);
            gameEvents.onHideMenu.RemoveListener(HideMenu);
            gameEvents.onNewGameStarted.RemoveListener(HideMenu);
        }

        private void ShowMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        private void HideMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(false);
            }
        }
    }
}