using Core.Events;
using UnityEngine;
using YG;

namespace View.UI
{
    /// <summary>
    /// Управляет видимостью окна статистики и обновлением таблицы лидеров.
    /// </summary>
    public class StatisticsWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject statisticsWindow;

        [Header("Leaderboard")]
        [SerializeField] private LeaderboardYG leaderboardYG;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (!gameEvents) return;
            gameEvents.onShowStatistics.AddListener(ShowStatisticsWindow);
            gameEvents.onHideStatistics.AddListener(HideStatisticsWindow);
        }

        private void Start()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (!gameEvents) return;
            gameEvents.onShowStatistics.RemoveListener(ShowStatisticsWindow);
            gameEvents.onHideStatistics.RemoveListener(HideStatisticsWindow);
        }

        /// <summary>
        /// Показывает окно статистики и запрашивает обновление таблицы лидеров.
        /// </summary>
        public void ShowStatisticsWindow()
        {
            if (!statisticsWindow) return;
            statisticsWindow.SetActive(true);
            if (leaderboardYG)
            {
                leaderboardYG.UpdateLB();
            }
        }

        /// <summary>
        /// Скрывает окно статистики.
        /// </summary>
        public void HideStatisticsWindow()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }
    }
}