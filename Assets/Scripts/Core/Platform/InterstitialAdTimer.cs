using Core.Events;
using UnityEngine;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Вызывает показ межстраничной рекламы по событию onAddExistingNumbers.
    /// Логика кулдауна и времени показа рекламы управляется самим плагином YG2.
    /// </summary>
    public class InterstitialAdTimer : MonoBehaviour
    {
        [Header("Каналы событий")]
        [Tooltip("Ссылка на ScriptableObject с игровыми событиями")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onAddExistingNumbers.AddListener(OnAddExistingNumbersTriggered);
            }
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onAddExistingNumbers.RemoveListener(OnAddExistingNumbersTriggered);
            }
        }

        /// <summary>
        /// Вызывается при срабатывании события onAddExistingNumbers.
        /// Запрашивает показ рекламы через плагин.
        /// </summary>
        private void OnAddExistingNumbersTriggered()
        {
            YG2.InterstitialAdvShow();
        }
    }
}