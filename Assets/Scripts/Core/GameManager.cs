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
            SubscribeToSaveEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromSaveEvents();
        }

        private void SubscribeToSaveEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.AddListener(SetTopLineVisibilityAndSave);
            _gameEvents.onCountersChanged.AddListener(data => SaveGame());
            _gameEvents.onDisableCountersConfirmed.AddListener(SaveGame);
            _gameEvents.onCellRemoved.AddListener(id => SaveGame());
            _gameEvents.onLineScoreAdded.AddListener(data => SaveGame());
            _gameEvents.onAddExistingNumbers.AddListener(SaveGame);
            _gameEvents.onUndoLastAction.AddListener(SaveGame);
        }

        private void UnsubscribeFromSaveEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.RemoveListener(SetTopLineVisibilityAndSave);
            _gameEvents.onCountersChanged.RemoveListener(data => SaveGame());
            _gameEvents.onDisableCountersConfirmed.RemoveListener(SaveGame);
            _gameEvents.onCellRemoved.RemoveListener(id => SaveGame());
            _gameEvents.onLineScoreAdded.RemoveListener(data => SaveGame());
            _gameEvents.onAddExistingNumbers.RemoveListener(SaveGame);
            _gameEvents.onUndoLastAction.RemoveListener(SaveGame);
        }

        private void SetTopLineVisibilityAndSave(bool isVisible)
        {
            _isTopLineVisible = isVisible;
            SaveGame();
        }

        public void SaveGame()
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