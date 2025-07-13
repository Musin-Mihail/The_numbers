using Core.Events;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Отвечает за обновление счета игрока в таблице лидеров при изменении статистики.
    /// </summary>
    public class LeaderboardUpdater : MonoBehaviour
    {
        private ILeaderboardService _leaderboardService;
        private GameEvents _gameEvents;

        /// <summary>
        /// Получает сервисы и подписывается на события.
        /// </summary>
        private void Start()
        {
            _leaderboardService = ServiceProvider.GetService<ILeaderboardService>();
            _gameEvents = ServiceProvider.GetService<GameEvents>();

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
        /// Обрабатывает изменение статистики и обновляет таблицу лидеров.
        /// </summary>
        /// <param name="statsData">Данные статистики (счет, множитель).</param>
        private void OnStatisticsChanged((long score, int multiplier) statsData)
        {
            var currentScore = (int)statsData.score;
            _leaderboardService?.UpdateLeaderboard(currentScore);
        }
    }
}