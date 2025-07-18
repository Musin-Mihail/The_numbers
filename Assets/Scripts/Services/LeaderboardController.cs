using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using View.UI;
using YG.Utils.LB;

namespace Services
{
    /// <summary>
    /// Управляет созданием и отображением записей в таблице лидеров.
    /// </summary>
    public class LeaderboardController : MonoBehaviour
    {
        [Tooltip("Префаб для одной строки в таблице лидеров")]
        [SerializeField] private LeaderboardEntry leaderboardEntryPrefab;
        [Tooltip("Контейнер, в который будут добавляться строки таблицы")]
        [SerializeField] private Transform container;

        /// <summary>
        /// Строит и отображает таблицу лидеров на основе полученных данных.
        /// </summary>
        /// <param name="lb">Данные таблицы лидеров от Yandex Games.</param>
        public void BuildLeaderboard(LBData lb)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            if (lb?.players == null || lb.players.Length == 0)
            {
                Debug.LogWarning("Данные таблицы лидеров пусты.");
                return;
            }

            var allPlayers = lb.players.ToList();
            var currentPlayerRank = lb.currentPlayer.rank;

            var playersToShow = new List<LBPlayerData>();
            var addedPlayerIDs = new HashSet<string>();

            // 1. Добавляем топ-3 игроков
            var top3Players = allPlayers.Where(p => p.rank <= 3).OrderBy(p => p.rank);
            foreach (var player in top3Players)
            {
                if (addedPlayerIDs.Add(player.uniqueID))
                {
                    playersToShow.Add(player);
                }
            }

            // 2. Добавляем текущего игрока и его соседей (3 выше, 3 ниже)
            var neighbors = allPlayers
                .Where(p => Mathf.Abs(p.rank - currentPlayerRank) <= 3)
                .OrderBy(p => p.rank);
            playersToShow.AddRange(neighbors.Where(player => addedPlayerIDs.Add(player.uniqueID)));

            // 3. Сортируем финальный список
            var finalDisplayList = playersToShow.OrderBy(p => p.rank).ToList();

            // 4. Создаем UI элементы
            int lastRank = 0;
            foreach (var player in finalDisplayList)
            {
                // Добавляем разделитель "---", если есть пропуск в рангах
                if (lastRank > 0 && player.rank > lastRank + 1)
                {
                    var separatorEntry = Instantiate(leaderboardEntryPrefab, container);
                    separatorEntry.SetAsSeparator();
                }

                var entryObject = Instantiate(leaderboardEntryPrefab, container);
                entryObject.Populate(player);

                lastRank = player.rank;
            }
        }
    }
}