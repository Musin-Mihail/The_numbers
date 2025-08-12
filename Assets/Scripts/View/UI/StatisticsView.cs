using Core;
using Core.Events;
using Localization;
using Model;
using TMPro;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Отображает игровую статистику (счет и множитель).
    /// </summary>
    public class StatisticsView : MonoBehaviour
    {
        [Header("UI Dependencies")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI multiplierText;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private LocalizationManager _localizationManager;
        private StatisticsModel _statisticsModel;

        private void OnEnable()
        {
            _localizationManager ??= ServiceProvider.GetService<LocalizationManager>();
            _statisticsModel ??= ServiceProvider.GetService<StatisticsModel>();

            if (gameEvents)
            {
                gameEvents.onStatisticsChanged.AddListener(UpdateStatisticsUI);
            }

            if (_statisticsModel != null)
            {
                UpdateStatisticsUI((_statisticsModel.Score, _statisticsModel.Multiplier));
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
        /// Обновляет UI статистики на основе полученных данных.
        /// Ручное обновление шрифтов больше не требуется.
        /// </summary>
        private void UpdateStatisticsUI((long score, int multiplier) data)
        {
            if (_localizationManager == null)
            {
                _localizationManager = ServiceProvider.GetService<LocalizationManager>();
                if (_localizationManager == null) return;
            }

            if (scoreText)
            {
                scoreText.text = string.Format(_localizationManager.Get("score"), data.score);
            }

            if (multiplierText)
            {
                multiplierText.text = string.Format(_localizationManager.Get("multiplier"), data.multiplier);
            }
        }
    }
}