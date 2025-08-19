using Core.Events;
using Interfaces;
using UnityEngine;
using YG;

namespace Core
{
    /// <summary>
    /// Отвечает за обновление счета игрока в таблице лидеров при изменении статистики,
    /// а также за обновление рекорда.
    /// </summary>
    public class LeaderboardUpdater : MonoBehaviour
    {
        private ILeaderboardService _leaderboardService;
        private GameEvents _gameEvents;

        /// <summary>
        /// Инициализация зависимостей, полученных из GameBootstrap.
        /// </summary>
        public void Initialize(ILeaderboardService leaderboardService, GameEvents gameEvents)
        {
            _leaderboardService = leaderboardService;
            _gameEvents = gameEvents;
        }

        /// <summary>
        /// Подписывается на события.
        /// </summary>
        private void OnEnable()
        {
            if (_gameEvents)
            {
                _gameEvents.onStatisticsChanged.AddListener(OnStatisticsChanged);
            }
        }

        /// <summary>
        /// Отписывается от событий.
        /// </summary>
        private void OnDisable()
        {
            if (_gameEvents)
            {
                _gameEvents.onStatisticsChanged.RemoveListener(OnStatisticsChanged);
            }
        }

        /// <summary>
        /// Обрабатывает изменение статистики, обновляет таблицу лидеров и рекорд.
        /// </summary>
        /// <param name="statsData">Данные статистики (счет, множитель).</param>
        private void OnStatisticsChanged((long score, int multiplier) statsData)
        {
            var currentScore = (int)statsData.score;
            _leaderboardService?.UpdateLeaderboard(currentScore);
            if (statsData.score <= YG2.saves.record) return;
            YG2.saves.record = statsData.score;
            Debug.Log($"Новый рекорд установлен: {YG2.saves.record}");
        }
    }
}