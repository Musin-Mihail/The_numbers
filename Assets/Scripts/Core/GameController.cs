using System;
using Core.Events;
using Core.Handlers;
using Gameplay;
using Model;
using View.Grid;

namespace Core
{
    public class GameController : IDisposable
    {
        private readonly GridModel _gridModel;
        private readonly ActionHistory _actionHistory;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly StatisticsModel _statisticsModel;
        private readonly GameEvents _gameEvents;
        private readonly GameManager _gameManager;
        private readonly GridView _gridView;

        private readonly MatchHandler _matchHandler;
        private readonly PlayerActionHandler _playerActionHandler;
        private readonly HintHandler _hintHandler;
        private readonly PlatformBridge _platformBridge;

        public GameController(
            GridModel gridModel,
            MatchValidator matchValidator,
            GameEvents gameEvents,
            ActionHistory actionHistory,
            ActionCountersModel actionCountersModel,
            StatisticsModel statisticsModel,
            GameManager gameManager,
            GridView gridView,
            IPlatformServices platformServices)
        {
            _gridModel = gridModel;
            _gameEvents = gameEvents;
            _actionHistory = actionHistory;
            _actionCountersModel = actionCountersModel;
            _statisticsModel = statisticsModel;
            _gameManager = gameManager;
            _gridView = gridView;

            _matchHandler = new MatchHandler(gridModel, matchValidator, statisticsModel, actionHistory, gameEvents, gameManager);
            _playerActionHandler = new PlayerActionHandler(actionCountersModel, actionHistory, gridModel, statisticsModel, gameEvents, gameManager);
            _hintHandler = new HintHandler(gridModel, matchValidator, actionCountersModel, gameEvents, gridView, gameManager);
            _platformBridge = new PlatformBridge(platformServices, gameEvents, actionCountersModel, gameManager);
        }

        public void Dispose()
        {
            _matchHandler?.Dispose();
            _playerActionHandler?.Dispose();
            _hintHandler?.Dispose();
            _platformBridge?.Dispose();
        }

        public void StartNewGame(bool resetStatisticsAndCounters = true)
        {
            _gridView.ResetSelectionAndHints();
            _gridModel.ClearField();
            _actionHistory.Clear();

            if (resetStatisticsAndCounters)
            {
                _actionCountersModel.ResetCounters();
                _statisticsModel.Reset();
            }

            _gameEvents.onCountersChanged.Raise(_actionCountersModel.AreCountersDisabled ? (-1, -1, -1) : (_actionCountersModel.UndoCount, _actionCountersModel.AddNumbersCount, _actionCountersModel.HintCount));
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));

            for (var i = 0; i < Constants.InitialLinesOnStart; i++)
            {
                _gridModel.CreateLine(i);
            }

            _gameEvents.onNewGameStarted.Raise();
            _gameManager?.RequestSave();
        }
    }
}