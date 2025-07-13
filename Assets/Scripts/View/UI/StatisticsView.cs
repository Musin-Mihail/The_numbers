using Core.Events;
using TMPro;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Отображает статистику игрока (счет и множитель) в UI.
    /// </summary>
    public class StatisticsView : MonoBehaviour
    {
        [Header("UI Dependencies")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI multiplierText;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onStatisticsChanged.AddListener(UpdateStatisticsUI);
            }
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onStatisticsChanged.RemoveListener(UpdateStatisticsUI);
            }
        }

        /// <summary>
        /// Обновляет текстовые поля со статистикой.
        /// </summary>
        /// <param name="data">Кортеж со счетом и множителем.</param>
        private void UpdateStatisticsUI((long score, int multiplier) data)
        {
            if (scoreText)
            {
                scoreText.text = $"Счет: {data.score}";
            }

            if (multiplierText)
            {
                multiplierText.text = $"Множитель: x{data.multiplier}";
            }
        }
    }
}