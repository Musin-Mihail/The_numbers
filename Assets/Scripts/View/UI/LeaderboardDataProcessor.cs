using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YG.Utils.LB;

namespace View.UI
{
    /// <summary>
    /// Обрабатывает данные таблицы лидеров: фильтрует, сортирует и формирует
    /// итоговый список игроков для отображения.
    /// </summary>
    public class LeaderboardDataProcessor
    {
        private const int MinEntriesToShow = 10;
        private const int PlayerNeighborsCount = 3;

        /// <summary>
        /// Обрабатывает сырые данные таблицы лидеров и возвращает отфильтрованный и отсортированный список.
        /// </summary>
        /// <param name="lb">Данные таблицы лидеров от Yandex Games.</param>
        /// <returns>Список игроков для отображения.</returns>
        public List<LBPlayerData> ProcessLeaderboardData(LBData lb)
        {
            var allPlayers = lb.players.ToList();
            var currentPlayerRank = lb.currentPlayer.rank;

            var addedPlayerIDs = new HashSet<string>();
            var playersToShow = new List<LBPlayerData>();

            // Добавляем топ-3 игроков
            var topPlayers = allPlayers.Where(p => p.rank <= 3).OrderBy(p => p.rank);
            playersToShow.AddRange(topPlayers.Where(player => addedPlayerIDs.Add(player.uniqueID)));

            // Добавляем соседей текущего игрока
            var neighbors = allPlayers
                .Where(p => Mathf.Abs(p.rank - currentPlayerRank) <= PlayerNeighborsCount)
                .OrderBy(p => p.rank);
            playersToShow.AddRange(neighbors.Where(player => addedPlayerIDs.Add(player.uniqueID)));

            // Добираем игроков до минимального количества, если необходимо
            if (playersToShow.Count < MinEntriesToShow && allPlayers.Count > playersToShow.Count)
            {
                var maxRankInList = playersToShow.Count > 0 ? playersToShow.Max(p => p.rank) : 0;
                var potentialAdditions = allPlayers
                    .Where(p => p.rank > maxRankInList)
                    .OrderBy(p => p.rank);

                foreach (var player in potentialAdditions)
                {
                    if (playersToShow.Count >= MinEntriesToShow) break;
                    if (addedPlayerIDs.Add(player.uniqueID))
                    {
                        playersToShow.Add(player);
                    }
                }
            }

            // Возвращаем финальный отсортированный список
            return playersToShow.OrderBy(p => p.rank).ToList();
        }
    }
}