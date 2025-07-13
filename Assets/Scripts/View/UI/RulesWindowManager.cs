using Core.Events;
using UnityEngine;

namespace View.UI
{
    public class RulesWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject rulesWindow;

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

        private void ShowRulesWindow()
        {
            if (rulesWindow)
            {
                rulesWindow.SetActive(true);
            }
        }

        private void HideRulesWindow()
        {
            if (rulesWindow)
            {
                rulesWindow.SetActive(false);
            }
        }
    }
}