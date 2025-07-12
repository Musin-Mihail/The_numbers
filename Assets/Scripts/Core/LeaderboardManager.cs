using Core.Events;
using UnityEngine;
using YG;

namespace Core
{
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Leaderboard Settings")]
        [SerializeField] private string leaderboardName = "TotalScore";

        private void OnEnable()
        {
            if (ServiceProvider.GetService<GameEvents>() != null)
            {
                ServiceProvider.GetService<GameEvents>().onStatisticsChanged.AddListener(OnStatisticsChanged);
            }
        }

        private void OnDisable()
        {
            if (ServiceProvider.GetService<GameEvents>() != null)
            {
                ServiceProvider.GetService<GameEvents>().onStatisticsChanged.RemoveListener(OnStatisticsChanged);
            }
        }

        private void OnStatisticsChanged((long score, int multiplier) statsData)
        {
            var currentScore = (int)statsData.score;
            UpdateLeaderboard(currentScore);
        }

        private void UpdateLeaderboard(int score)
        {
            if (YG2.player.auth)
            {
                YG2.SetLeaderboard(leaderboardName, score);
                Debug.Log($"Leaderboard '{leaderboardName}' updated with score: {score}");
            }
            else
            {
                Debug.LogWarning("Player is not authorized. Score not sent to leaderboard.");
            }
        }
    }
}