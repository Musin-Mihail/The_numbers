using Core;
using Core.Events;
using Services;
using UnityEngine;
using YG;
using YG.Utils.LB;

namespace View.UI
{
    /// <summary>
    /// Управляет видимостью окна статистики и обновлением таблицы лидеров.
    /// </summary>
    public class StatisticsWindowManager : MonoBehaviour
    {
        [Tooltip("Объект окна статистики, который будет показан/скрыт")]
        [SerializeField] private GameObject statisticsWindow;

        [Header("Leaderboard")]
        [Tooltip("Контроллер, отвечающий за создание и отображение таблицы лидеров")]
        [SerializeField] private LeaderboardController leaderboardController;

        [Header("Event Listening")]
        [Tooltip("Контейнер для игровых событий")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onShowStatistics.AddListener(ShowStatisticsWindow);
                gameEvents.onHideStatistics.AddListener(HideStatisticsWindow);
            }

            YG2.onGetLeaderboard += OnLeaderboardReceived;
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onShowStatistics.RemoveListener(ShowStatisticsWindow);
                gameEvents.onHideStatistics.RemoveListener(HideStatisticsWindow);
            }

            YG2.onGetLeaderboard -= OnLeaderboardReceived;
        }

        private void Start()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }

        /// <summary>
        /// Показывает окно статистики и запрашивает обновление таблицы лидеров.
        /// </summary>
        private void ShowStatisticsWindow()
        {
            if (!statisticsWindow) return;
            statisticsWindow.SetActive(true);

            if (YG2.player.auth)
            {
                YG2.GetLeaderboard(Constants.LeaderboardName, 10, 3);
            }
            else
            {
                Debug.LogWarning("Player is not authorized. Cannot fetch leaderboard.");
            }
        }

        /// <summary>
        /// Метод-обработчик, который вызывается после получения данных от YG2.
        /// </summary>
        /// <param name="lb">Данные таблицы лидеров типа LBData.</param>
        private void OnLeaderboardReceived(LBData lb)
        {
            if (lb.technoName != Constants.LeaderboardName) return;

            if (leaderboardController)
            {
                leaderboardController.BuildLeaderboard(lb);
            }
            else
            {
                Debug.LogError("LeaderboardController не назначен в инспекторе!", this);
            }
        }

        /// <summary>
        /// Скрывает окно статистики.
        /// </summary>
        private void HideStatisticsWindow()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }
    }
}