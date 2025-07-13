using Core.Events;
using UnityEngine;

namespace Core
{
    public class LeaderboardUpdater : MonoBehaviour
    {
        private ILeaderboardService _leaderboardService;
        private GameEvents _gameEvents;

        private void Start()
        {
            _leaderboardService = ServiceProvider.GetService<ILeaderboardService>();
            _gameEvents = ServiceProvider.GetService<GameEvents>();

            if (_gameEvents)
            {
                _gameEvents.onStatisticsChanged.AddListener(OnStatisticsChanged);
            }
        }

        private void OnDisable()
        {
            if (_gameEvents)
            {
                _gameEvents.onStatisticsChanged.RemoveListener(OnStatisticsChanged);
            }
        }

        private void OnStatisticsChanged((long score, int multiplier) statsData)
        {
            var currentScore = (int)statsData.score;
            _leaderboardService?.UpdateLeaderboard(currentScore);
        }
    }
}