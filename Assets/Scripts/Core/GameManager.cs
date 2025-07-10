using System;
using System.Collections;
using System.Linq;
using Core.Events;
using Model;
using UnityEngine;
using View.Grid;
using YG;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private GridModel _gridModel;
        private StatisticsModel _statisticsModel;
        private ActionCountersModel _actionCountersModel;
        private GameEvents _gameEvents;
        private bool _isLoading;
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
                SaveGame();
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
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void SubscribeToEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.AddListener(SetTopLineVisibilityAndSave);
            _gameEvents.onBoardCleared.AddListener(HandleBoardCleared);
        }

        private void UnsubscribeFromEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.RemoveListener(SetTopLineVisibilityAndSave);
            _gameEvents.onBoardCleared.RemoveListener(HandleBoardCleared);
        }

        private void HandleBoardCleared()
        {
            StartCoroutine(BoardClearedRoutine());
        }

        private IEnumerator BoardClearedRoutine()
        {
            yield return new WaitForSeconds(2.0f);

            var gameController = ServiceProvider.GetService<GameController>();
            gameController?.StartNewGame(false);
        }

        private void SetTopLineVisibilityAndSave(bool isVisible)
        {
            if (YG2.saves.isTopLineVisible == isVisible) return;
            YG2.saves.isTopLineVisible = isVisible;
            RequestSave();
        }

        public void RequestSave()
        {
            if (_isSaving) return;

            if (_timeSinceLastSave >= SaveCooldown)
            {
                SaveGame();
            }
            else
            {
                _savePending = true;
            }
        }

        private void SaveGame()
        {
            if (_isLoading || _isSaving) return;

            _isSaving = true;
            _savePending = false;
            _timeSinceLastSave = 0f;

            YG2.saves.gridCells = _gridModel.GetAllCellData().Select(c => new CellDataSerializable(c)).ToList();
            YG2.saves.statistics = new StatisticsModelSerializable(_statisticsModel);
            YG2.saves.actionCounters = new ActionCountersModelSerializable(_actionCountersModel);

            YG2.saves.isGameEverSaved = true;

            YG2.SaveProgress();

            _isSaving = false;
            Debug.Log("Game data save requested via PluginYG2.");
        }

        public void LoadGame(Action<bool> onComplete)
        {
            _isLoading = true;

            if (YG2.saves.isGameEverSaved)
            {
                _gridModel.RestoreState(YG2.saves.gridCells);
                _statisticsModel.SetState(YG2.saves.statistics.score, YG2.saves.statistics.multiplier);
                _actionCountersModel.RestoreState(YG2.saves.actionCounters);

                var gridView = ServiceProvider.GetService<GridView>();
                if (gridView)
                {
                    gridView.FullRedraw();
                }

                if (_gameEvents)
                {
                    _gameEvents.onToggleTopLine?.Raise(YG2.saves.isTopLineVisible);
                    if (YG2.saves.actionCounters.areCountersDisabled)
                    {
                        _gameEvents.onCountersChanged?.Raise((-1, -1, -1));
                    }
                    else
                    {
                        _gameEvents.onCountersChanged?.Raise((YG2.saves.actionCounters.undoCount, YG2.saves.actionCounters.addNumbersCount, YG2.saves.actionCounters.hintCount));
                    }

                    _gameEvents.onStatisticsChanged?.Raise((YG2.saves.statistics.score, YG2.saves.statistics.multiplier));
                }

                Debug.Log("Game data loaded successfully from YG2.saves.");
                _isLoading = false;
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("No save data found in YG2.saves. A new game will be started.");
                _isLoading = false;
                onComplete?.Invoke(false);
            }
        }
    }
}