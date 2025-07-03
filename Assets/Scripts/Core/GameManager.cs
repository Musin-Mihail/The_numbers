using System.Linq;
using Core.Events;
using Model;
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

        private void Awake()
        {
            _gridModel = ServiceProvider.GetService<GridModel>();
            _statisticsModel = ServiceProvider.GetService<StatisticsModel>();
            _actionCountersModel = ServiceProvider.GetService<ActionCountersModel>();
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            SubscribeToEvents();
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
            SaveGame();
        }

        private void SaveGame()
        {
            if (_isLoading) return;

            var gameData = new GameData
            {
                gridCells = _gridModel.GetAllCellData().Select(c => new CellDataSerializable(c)).ToList(),
                statistics = new StatisticsModelSerializable(_statisticsModel),
                actionCounters = new ActionCountersModelSerializable(_actionCountersModel),
                isTopLineVisible = _isTopLineVisible
            };

            SaveLoadService.SaveGame(gameData);
        }

        public bool LoadGame()
        {
            var data = SaveLoadService.LoadGame();
            if (data == null) return false;

            _isLoading = true;

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

            return true;
        }
    }
}