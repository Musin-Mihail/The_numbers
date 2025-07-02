using Core.Events;
using TMPro;
using UnityEngine;

namespace View.UI
{
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

        private void Start()
        {
            UpdateStatisticsUI((0, 1));
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onStatisticsChanged.RemoveListener(UpdateStatisticsUI);
            }
        }

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