using Core.Events;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Управляет видимостью главного меню.
    /// </summary>
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

        /// <summary>
        /// Показывает окно меню.
        /// </summary>
        public void ShowMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        /// <summary>
        /// Скрывает окно меню.
        /// </summary>
        public void HideMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(false);
            }
        }
    }
}