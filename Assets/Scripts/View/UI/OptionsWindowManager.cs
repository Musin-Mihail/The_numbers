using Core.Events;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Управляет видимостью окна настроек.
    /// </summary>
    public class OptionsWindowManager : MonoBehaviour
    {
        [Tooltip("Объект окна настроек, который будет показан/скрыт")]
        [SerializeField] private GameObject optionsWindow;

        [Header("Каналы событий")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (!gameEvents) return;
            gameEvents.onShowOptions.AddListener(ShowOptionsWindow);
            gameEvents.onHideOptions.AddListener(HideOptionsWindow);
        }

        private void Start()
        {
            if (optionsWindow)
            {
                optionsWindow.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (!gameEvents) return;
            gameEvents.onShowOptions.RemoveListener(ShowOptionsWindow);
            gameEvents.onHideOptions.RemoveListener(HideOptionsWindow);
        }

        /// <summary>
        /// Показывает окно настроек.
        /// </summary>
        private void ShowOptionsWindow()
        {
            if (optionsWindow)
            {
                optionsWindow.SetActive(true);
            }
        }

        /// <summary>
        /// Скрывает окно настроек.
        /// </summary>
        private void HideOptionsWindow()
        {
            if (optionsWindow)
            {
                optionsWindow.SetActive(false);
            }
        }
    }
}