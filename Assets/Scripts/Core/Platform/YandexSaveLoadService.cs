using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Events;
using Interfaces;
using Model;
using UnityEngine;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Реализация сервиса сохранения и загрузки для платформы Yandex Games.
    /// </summary>
    public class YandexSaveLoadService : ISaveLoadService
    {
        private readonly GridModel _gridModel;
        private readonly StatisticsModel _statisticsModel;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly GameEvents _gameEvents;

        private bool _isLoading;
        private bool _isSaving;

        /// <summary>
        /// Инициализирует сервис сохранения/загрузки с необходимыми моделями.
        /// </summary>
        public YandexSaveLoadService(GridModel gridModel, StatisticsModel statisticsModel, ActionCountersModel actionCountersModel, GameEvents gameEvents)
        {
            _gridModel = gridModel;
            _statisticsModel = statisticsModel;
            _actionCountersModel = actionCountersModel;
            _gameEvents = gameEvents;
        }

        /// <summary>
        /// Запрашивает сохранение игры.
        /// </summary>
        public void RequestSave()
        {
            SaveGame();
        }

        /// <summary>
        /// Сохраняет игру, используя новый компактный формат для сетки.
        /// </summary>
        private void SaveGame()
        {
            if (_isLoading || _isSaving) return;
            _isSaving = true;
            var gridStringBuilder = new StringBuilder();
            var grid = _gridModel.Cells;
            for (var lineIndex = 0; lineIndex < grid.Count; lineIndex++)
            {
                var line = grid[lineIndex];
                var lineParts = (from cell in line where cell.IsActive select $"{cell.Column}:{cell.Number}").ToList();
                if (lineParts.Count > 0)
                {
                    gridStringBuilder.Append(string.Join(",", lineParts));
                }

                if (lineIndex < grid.Count - 1)
                {
                    gridStringBuilder.Append('|');
                }
            }

            YG2.saves.gridState = gridStringBuilder.ToString();

#pragma warning disable 0618
            if (YG2.saves.gridCells != null)
            {
                YG2.saves.gridCells.Clear();
            }
#pragma warning restore 0618
            YG2.saves.statistics = new StatisticsModelSerializable(_statisticsModel);
            YG2.saves.actionCounters = new ActionCountersModelSerializable(_actionCountersModel);
            try
            {
                var jsonSaves = JsonUtility.ToJson(YG2.saves);
                var sizeInBytes = Encoding.UTF8.GetByteCount(jsonSaves);
                var sizeInKilobytes = sizeInBytes / 1024f;
                Debug.Log($"Размер сохранения: {sizeInKilobytes:F2} КБ. Строка сетки: {YG2.saves.gridState.Length} символов.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при расчете размера сохранения: {e.Message}");
            }

            Debug.Log("Запрос на сохранение игровых данных через YandexSaveLoadService.");
            YG2.SaveProgress();
            _isSaving = false;
        }


        /// <summary>
        /// Загружает игру с серверов Yandex, поддерживая старый и новый форматы.
        /// </summary>
        /// <param name="onComplete">Callback, вызываемый по завершении. True при успехе.</param>
        public void LoadGame(Action<bool> onComplete)
        {
            _isLoading = true;
            var savedCells = new List<CellDataSerializable>();
#pragma warning disable 0618
            if (YG2.saves.gridCells != null && YG2.saves.gridCells.Count > 0)
            {
                Debug.Log("Обнаружены старые данные сохранения (gridCells). Производится миграция на новый формат.");
                savedCells = YG2.saves.gridCells;
            }
#pragma warning restore 0618
            else if (!string.IsNullOrEmpty(YG2.saves.gridState))
            {
                Debug.Log("Загрузка из нового компактного формата (gridState).");
                var lines = YG2.saves.gridState.Split('|');
                for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    var lineData = lines[lineIndex];
                    if (string.IsNullOrEmpty(lineData)) continue;
                    var cellParts = lineData.Split(',');
                    foreach (var part in cellParts)
                    {
                        try
                        {
                            var pair = part.Split(':');
                            if (pair.Length != 2) continue;
                            var column = int.Parse(pair[0]);
                            var number = int.Parse(pair[1]);
                            savedCells.Add(new CellDataSerializable
                            {
                                number = number,
                                line = lineIndex,
                                column = column,
                                isActive = true
                            });
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Ошибка парсинга данных ячейки '{part}': {e.Message}");
                        }
                    }
                }
            }

            _gridModel.RestoreState(savedCells);
            _statisticsModel.SetState(YG2.saves.statistics.score, YG2.saves.statistics.multiplier);
            _actionCountersModel.RestoreState(YG2.saves.actionCounters);
            if (_gameEvents)
            {
                _gameEvents.onToggleTopLine?.Raise(YG2.saves.isTopLineVisible);
                _gameEvents.onStatisticsChanged?.Raise((YG2.saves.statistics.score, YG2.saves.statistics.multiplier));
            }

            Debug.Log("Игровые данные успешно загружены из сохранений Yandex.");
            _isLoading = false;
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Устанавливает видимость верхней строки и сохраняет состояние.
        /// </summary>
        public void SetTopLineVisibility(bool isVisible)
        {
            YG2.saves.isTopLineVisible = isVisible;
        }
    }
}