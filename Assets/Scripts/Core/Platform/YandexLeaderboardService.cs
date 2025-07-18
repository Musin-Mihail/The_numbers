using Interfaces;
using UnityEngine;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Реализация сервиса таблицы лидеров для Yandex Games.
    /// </summary>
    public class YandexLeaderboardService : ILeaderboardService
    {
        private readonly string _leaderboardName;

        /// <summary>
        /// Инициализирует сервис таблицы лидеров с указанным именем.
        /// </summary>
        /// <param name="leaderboardName">Имя таблицы лидеров в Yandex Games Console.</param>
        public YandexLeaderboardService(string leaderboardName)
        {
            _leaderboardName = leaderboardName;
        }

        /// <summary>
        /// Обновляет счет в таблице лидеров Yandex.
        /// </summary>
        public void UpdateLeaderboard(int score)
        {
            if (YG2.player.auth)
            {
                YG2.SetLeaderboard(_leaderboardName, score);
                Debug.Log($"Таблица лидеров '{_leaderboardName}' обновлена с результатом: {score}");
            }
            else
            {
                Debug.LogWarning("Игрок не авторизован. Результат не отправлен в таблицу лидеров.");
            }
        }
    }
}