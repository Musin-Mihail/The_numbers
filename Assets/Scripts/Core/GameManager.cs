using System;
using System.Linq;
using Core.Events;
using Model;
using PlayablesStudio.Plugins.YandexGamesSDK.Runtime;
using UnityEngine;
using View.Grid;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private GridModel _gridModel;
        private StatisticsModel _statisticsModel;
        private ActionCountersModel _actionCountersModel;
        private GameEvents _gameEvents;

        private bool _isTopLineVisible = true;
        private bool _isLoading;
        private const string GameDataKey = "GameData";

        private const float SaveCooldown = 5.0f;
        private float _timeSinceLastSave = 5.0f;
        private bool _savePending;
        private bool _isSaving;

        private void Awake()
        {
            _gridModel = ServiceProvider.GetService<GridModel>();
            _statisticsModel = ServiceProvider.GetService<StatisticsModel>();
            _actionCountersModel = ServiceProvider.GetService<ActionCountersModel>();
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            SubscribeToEvents();
        }

        private void Update()
        {
            if (_timeSinceLastSave < SaveCooldown)
            {
                _timeSinceLastSave += Time.deltaTime;
            }

            if (_savePending && _timeSinceLastSave >= SaveCooldown && !_isSaving)
            {
                SaveGame(true);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame(true);
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame(false);
        }

        private void SubscribeToEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.AddListener(SetTopLineVisibilityAndSave);
        }

        private void UnsubscribeFromEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.RemoveListener(SetTopLineVisibilityAndSave);
        }

        private void SetTopLineVisibilityAndSave(bool isVisible)
        {
            if (_isTopLineVisible == isVisible) return;
            _isTopLineVisible = isVisible;
            RequestSave();
        }

        public void RequestSave()
        {
            if (_isSaving) return;

            if (_timeSinceLastSave >= SaveCooldown)
            {
                SaveGame(true);
            }
            else
            {
                _savePending = true;
            }
        }

        private void SaveGame(bool flushToCloud)
        {
            if (_isLoading || _isSaving) return;

            _isSaving = true;
            _savePending = false;
            _timeSinceLastSave = 0f;

            var gameData = new GameData
            {
                gridCells = _gridModel.GetAllCellData().Select(c => new CellDataSerializable(c)).ToList(),
                statistics = new StatisticsModelSerializable(_statisticsModel),
                actionCounters = new ActionCountersModelSerializable(_actionCountersModel),
                isTopLineVisible = _isTopLineVisible
            };

            YandexGamesSDK.Instance.CloudStorage.Save(GameDataKey, gameData, (success, error) =>
            {
                if (!success || !flushToCloud)
                {
                    _isSaving = false;
                    return;
                }

                YandexGamesSDK.Instance.CloudStorage.FlushData((flushSuccess, flushError) => { _isSaving = false; });
            });
        }

        public void LoadGame(Action<bool> onComplete)
        {
            _isLoading = true;

            YandexGamesSDK.Instance.CloudStorage.Load<GameData>(GameDataKey, (success, data, error) =>
            {
                if (success && data != null)
                {
                    _gridModel.RestoreState(data.gridCells);
                    _statisticsModel.SetState(data.statistics.score, data.statistics.multiplier);
                    _actionCountersModel.RestoreState(data.actionCounters);
                    _isTopLineVisible = data.isTopLineVisible;

                    var gridView = ServiceProvider.GetService<GridView>();
                    if (gridView)
                    {
                        gridView.FullRedraw();
                    }

                    if (_gameEvents)
                    {
                        _gameEvents.onToggleTopLine?.Raise(_isTopLineVisible);
                        if (data.actionCounters.areCountersDisabled)
                        {
                            _gameEvents.onCountersChanged?.Raise((-1, -1, -1));
                        }
                        else
                        {
                            _gameEvents.onCountersChanged?.Raise((data.actionCounters.undoCount, data.actionCounters.addNumbersCount, data.actionCounters.hintCount));
                        }

                        _gameEvents.onStatisticsChanged?.Raise((data.statistics.score, data.statistics.multiplier));
                    }

                    _isLoading = false;
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogWarning($"Failed to load data from cloud: {error}. A new game will be started.");
                    _isLoading = false;
                    onComplete?.Invoke(false);
                }
            });
        }
    }
}