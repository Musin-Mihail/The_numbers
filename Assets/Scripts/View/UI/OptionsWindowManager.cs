using Core.Events;
using UnityEngine;
using UnityEngine.UI;
using YG;

namespace View.UI
{
    /// <summary>
    /// Управляет видимостью окна настроек и состоянием его элементов.
    /// </summary>
    public class OptionsWindowManager : MonoBehaviour
    {
        [Tooltip("Объект окна настроек, который будет показан/скрыт")]
        [SerializeField] private GameObject optionsWindow;
        [SerializeField] private Toggle topLineToggle;

        [Header("Каналы событий")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (!gameEvents) return;
            gameEvents.onShowOptions.AddListener(ShowOptionsWindow);
            gameEvents.onHideOptions.AddListener(HideOptionsWindow);

            if (!topLineToggle) return;
            topLineToggle.isOn = YG2.saves.isTopLineVisible;
            topLineToggle.onValueChanged.AddListener(OnToggleValueChanged);
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

            if (topLineToggle)
            {
                topLineToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }

        /// <summary>
        /// Вызывается, когда пользователь изменяет состояние Toggle.
        /// </summary>
        /// <param name="isOn">Новое состояние.</param>
        private void OnToggleValueChanged(bool isOn)
        {
            gameEvents.onToggleTopLine.Raise(isOn);
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