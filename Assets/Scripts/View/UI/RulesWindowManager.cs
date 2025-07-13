using Core.Events;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Управляет видимостью окна с правилами игры.
    /// </summary>
    public class RulesWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject rulesWindow;
        [SerializeField] private RulesGrid rulesGrid;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (!gameEvents) return;
            gameEvents.onShowRules.AddListener(ShowRulesWindow);
            gameEvents.onHideRules.AddListener(HideRulesWindow);
        }

        private void Start()
        {
            if (rulesWindow)
            {
                rulesWindow.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (!gameEvents) return;
            gameEvents.onShowRules.RemoveListener(ShowRulesWindow);
            gameEvents.onHideRules.RemoveListener(HideRulesWindow);
        }

        /// <summary>
        /// Показывает окно правил и генерирует демонстрационную сетку.
        /// </summary>
        private void ShowRulesWindow()
        {
            if (!rulesWindow) return;
            rulesWindow.SetActive(true);
            if (rulesGrid)
            {
                rulesGrid.GenerateGrid();
            }
            else
            {
                Debug.LogError("RulesGrid is not assigned in the inspector for RulesWindowManager.");
            }
        }

        /// <summary>
        /// Скрывает окно правил.
        /// </summary>
        private void HideRulesWindow()
        {
            if (rulesWindow)
            {
                rulesWindow.SetActive(false);
            }
        }
    }
}