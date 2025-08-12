using System.Collections.Generic;
using UnityEngine;
using YG;
using YG.Utils.LB;

namespace View.UI
{
    /// <summary>
    /// Управляет отображением записей в таблице лидеров, используя пул объектов.
    /// Логика обработки данных делегирована классу LeaderboardDataProcessor.
    /// </summary>
    public class LeaderboardView : MonoBehaviour
    {
        [Tooltip("Префаб для одной строки в таблице лидеров")]
        [SerializeField] private LeaderboardEntry leaderboardEntryPrefab;
        [Tooltip("Контейнер, в который будут добавляться строки таблицы")]
        [SerializeField] private Transform container;

        private readonly List<LeaderboardEntry> _activeEntries = new();
        private readonly Queue<LeaderboardEntry> _pooledEntries = new();
        private LeaderboardDataProcessor _dataProcessor;

        private void Awake()
        {
            _dataProcessor = new LeaderboardDataProcessor();
        }

        /// <summary>
        /// Строит и отображает таблицу лидеров на основе полученных данных.
        /// </summary>
        /// <param name="lb">Данные таблицы лидеров от Yandex Games.</param>
        public void BuildLeaderboard(LBData lb)
        {
            ReturnAllEntriesToPool();

            if (lb?.players == null || lb.players.Length == 0)
            {
                Debug.LogWarning("Данные таблицы лидеров пусты.");
                return;
            }

            var finalDisplayList = _dataProcessor.ProcessLeaderboardData(lb);
            var currentPlayerId = YG2.player.id;

            var lastRank = 0;
            foreach (var player in finalDisplayList)
            {
                if (lastRank > 0 && player.rank > lastRank + 1)
                {
                    var separatorEntry = GetEntryFromPool();
                    separatorEntry.SetAsSeparator();
                }

                var entryObject = GetEntryFromPool();
                // Проверяем, является ли запись записью текущего игрока
                var isCurrentPlayer = !string.IsNullOrEmpty(currentPlayerId) && player.uniqueID == currentPlayerId;
                entryObject.Populate(player, isCurrentPlayer);
                lastRank = player.rank;
            }
        }

        /// <summary>
        /// Возвращает все активные UI-элементы в пул.
        /// </summary>
        private void ReturnAllEntriesToPool()
        {
            foreach (var entry in _activeEntries)
            {
                entry.gameObject.SetActive(false);
                _pooledEntries.Enqueue(entry);
            }

            _activeEntries.Clear();
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