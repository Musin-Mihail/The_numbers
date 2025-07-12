using Core.Events;
using UnityEngine;
using YG;

namespace View.UI
{
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

        private void ShowStatisticsWindow()
        {
            if (!statisticsWindow) return;
            statisticsWindow.SetActive(true);
            if (leaderboardYG)
            {
                leaderboardYG.UpdateLB();
            }
        }

        private void HideStatisticsWindow()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }
    }
}