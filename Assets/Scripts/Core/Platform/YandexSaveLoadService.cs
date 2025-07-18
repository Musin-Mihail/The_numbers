using System;
using System.Linq;
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

        private void SaveGame()
        {
            if (_isLoading || _isSaving) return;

            _isSaving = true;

            YG2.saves.gridCells = _gridModel.GetAllCellData().Select(c => new CellDataSerializable(c)).ToList();
            YG2.saves.statistics = new StatisticsModelSerializable(_statisticsModel);
            YG2.saves.actionCounters = new ActionCountersModelSerializable(_actionCountersModel);
            YG2.saves.isGameEverSaved = true;
            Debug.Log("Запрос на сохранение игровых данных через YandexSaveLoadService.");
            YG2.SaveProgress();
            _isSaving = false;
        }

        /// <summary>
        /// Загружает игру с серверов Yandex.
        /// </summary>
        /// <param name="onComplete">Callback, вызываемый по завершении. True при успехе.</param>
        public void LoadGame(Action<bool> onComplete)
        {
            _isLoading = true;

            if (YG2.saves.isGameEverSaved)
            {
                _gridModel.RestoreState(YG2.saves.gridCells);
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
            else
            {
                Debug.LogWarning("Данные сохранения в Yandex не найдены");
                _isLoading = false;
                onComplete?.Invoke(false);
            }
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