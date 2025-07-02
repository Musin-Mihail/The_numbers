using Core;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class StatisticsView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI multiplierText;

        private void Start()
        {
            GameEvents.OnStatisticsChanged += UpdateStatisticsUI;
            UpdateStatisticsUI(0, 1);
        }

        private void OnDestroy()
        {
            GameEvents.OnStatisticsChanged -= UpdateStatisticsUI;
        }

        private void UpdateStatisticsUI(long score, int multiplier)
        {
            if (scoreText)
            {
                scoreText.text = $"Счет: {score}";
            }

            if (multiplierText)
            {
                multiplierText.text = $"Множитель: x{multiplier}";
            }
        }
    }
}