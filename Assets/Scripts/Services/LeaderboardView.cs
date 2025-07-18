using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using View.UI;
using YG.Utils.LB;

namespace Services
{
    /// <summary>
    /// Управляет созданием и отображением записей в таблице лидеров с использованием пула объектов.
    /// </summary>
    public class LeaderboardView : MonoBehaviour
    {
        [Tooltip("Префаб для одной строки в таблице лидеров")]
        [SerializeField] private LeaderboardEntry leaderboardEntryPrefab;
        [Tooltip("Контейнер, в который будут добавляться строки таблицы")]
        [SerializeField] private Transform container;

        private readonly List<LeaderboardEntry> _activeEntries = new();
        private readonly Queue<LeaderboardEntry> _pooledEntries = new();

        /// <summary>
        /// Строит и отображает таблицу лидеров на основе полученных данных.
        /// </summary>
        /// <param name="lb">Данные таблицы лидеров от Yandex Games.</param>
        public void BuildLeaderboard(LBData lb)
        {
            foreach (var entry in _activeEntries)
            {
                entry.gameObject.SetActive(false);
                _pooledEntries.Enqueue(entry);
            }

            _activeEntries.Clear();

            if (lb?.players == null || lb.players.Length == 0)
            {
                Debug.LogWarning("Данные таблицы лидеров пусты.");
                return;
            }

            const int minEntries = 10;
            var allPlayers = lb.players.ToList();
            var currentPlayerRank = lb.currentPlayer.rank;
            var addedPlayerIDs = new HashSet<string>();
            var top3Players = allPlayers.Where(p => p.rank <= 3).OrderBy(p => p.rank);
            var playersToShow = top3Players.Where(player => addedPlayerIDs.Add(player.uniqueID)).ToList();
            var neighbors = allPlayers
                .Where(p => Mathf.Abs(p.rank - currentPlayerRank) <= 3)
                .OrderBy(p => p.rank);
            playersToShow.AddRange(neighbors.Where(player => addedPlayerIDs.Add(player.uniqueID)));
            var finalDisplayList = playersToShow.OrderBy(p => p.rank).ToList();
            if (finalDisplayList.Count < minEntries && allPlayers.Count > finalDisplayList.Count)
            {
                var maxRankInList = finalDisplayList.Any() ? finalDisplayList.Max(p => p.rank) : 0;
                var potentialAdditions = allPlayers
                    .Where(p => p.rank > maxRankInList)
                    .OrderBy(p => p.rank);

                foreach (var player in potentialAdditions)
                {
                    if (finalDisplayList.Count >= minEntries) break;
                    if (!addedPlayerIDs.Add(player.uniqueID)) continue;
                    finalDisplayList.Add(player);
                }

                finalDisplayList = finalDisplayList.OrderBy(p => p.rank).ToList();
            }

            var lastRank = 0;
            foreach (var player in finalDisplayList)
            {
                if (lastRank > 0 && player.rank > lastRank + 1)
                {
                    var separatorEntry = GetEntryFromPool();
                    separatorEntry.SetAsSeparator();
                }

                var entryObject = GetEntryFromPool();
                entryObject.Populate(player);
                lastRank = player.rank;
            }
        }

        /// <summary>
        /// Получает строку из пула или создает новую, если пул пуст.
        /// </summary>
        /// <returns>Активный и готовый к использованию экземпляр LeaderboardEntry.</returns>
        private LeaderboardEntry GetEntryFromPool()
        {
            LeaderboardEntry entry;
            entry = _pooledEntries.Count > 0 ? _pooledEntries.Dequeue() : Instantiate(leaderboardEntryPrefab, container);
            entry.gameObject.SetActive(true);
            _activeEntries.Add(entry);
            return entry;
        }
    }
}