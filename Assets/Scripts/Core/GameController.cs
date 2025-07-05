using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Gameplay;
using Model;

namespace Core
{
    public class GameController
    {
        private GridModel _gridModel;
        private MatchValidator _matchValidator;
        private ActionHistory _actionHistory;
        private ActionCountersModel _actionCountersModel;
        private StatisticsModel _statisticsModel;
        private GameEvents _gameEvents;
        private GameManager _gameManager;
        private const int InitialQuantityByHeight = 5;

        public GameController()
        {
            Initialize();
        }

        private void Initialize()
        {
            _gridModel = ServiceProvider.GetService<GridModel>();
            _matchValidator = ServiceProvider.GetService<MatchValidator>();
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            _actionHistory = ServiceProvider.GetService<ActionHistory>();
            _actionCountersModel = ServiceProvider.GetService<ActionCountersModel>();
            _statisticsModel = ServiceProvider.GetService<StatisticsModel>();
            _gameManager = ServiceProvider.GetService<GameManager>();
            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            _gameEvents.onAttemptMatch.AddListener(AttemptMatch);
            _gameEvents.onAddExistingNumbers.AddListener(AddExistingNumbersAsNewLines);
            _gameEvents.onUndoLastAction.AddListener(UndoLastAction);
            _gameEvents.onRequestHint.AddListener(FindAndShowHint);
            _gameEvents.onRefillCountersConfirmed.AddListener(HandleRefillCountersConfirmed);
            _gameEvents.onDisableCountersConfirmed.AddListener(HandleDisableCountersConfirmed);
        }

        private void UnsubscribeFromInputEvents()
        {
            _gameEvents.onAttemptMatch.RemoveListener(AttemptMatch);
            _gameEvents.onAddExistingNumbers.RemoveListener(AddExistingNumbersAsNewLines);
            _gameEvents.onUndoLastAction.RemoveListener(UndoLastAction);
            _gameEvents.onRequestHint.RemoveListener(FindAndShowHint);
            _gameEvents.onRefillCountersConfirmed.RemoveListener(HandleRefillCountersConfirmed);
            _gameEvents.onDisableCountersConfirmed.RemoveListener(HandleDisableCountersConfirmed);
        }

        private void HandleRefillCountersConfirmed()
        {
            _actionCountersModel.ResetCounters();
            RaiseCountersChangedEvent();
            _gameManager?.RequestSave();
        }

        private void RaiseCountersChangedEvent()
        {
            _gameEvents.onCountersChanged.Raise(_actionCountersModel.AreCountersDisabled ? (-1, -1, -1) : (_actionCountersModel.UndoCount, _actionCountersModel.AddNumbersCount, _actionCountersModel.HintCount));
        }

        private void HandleDisableCountersConfirmed()
        {
            _actionCountersModel.DisableCounters();
            RaiseCountersChangedEvent();
            _gameManager?.RequestSave();
        }

        private void FindAndShowHint()
        {
            if (!_actionCountersModel.IsHintAvailable())
            {
                _gameEvents.onRequestRefillCounters.Raise();
                return;
            }

            var activeCells = _gridModel.GetAllActiveCellData();
            if (activeCells.Count < 2)
            {
                _gameEvents.onNoHintFound.Raise();
                return;
            }

            for (var i = 0; i < activeCells.Count; i++)
            {
                for (var j = i + 1; j < activeCells.Count; j++)
                {
                    var cell1 = activeCells[i];
                    var cell2 = activeCells[j];

                    if (!_matchValidator.IsAValidMatch(cell1, cell2)) continue;
                    ShowHintAndDecrementCounter(cell1, cell2);
                    return;
                }
            }

            _gameEvents.onNoHintFound.Raise();
        }

        private void ShowHintAndDecrementCounter(CellData cell1, CellData cell2)
        {
            if (!_actionCountersModel.AreCountersDisabled)
            {
                _actionCountersModel.DecrementHint();
            }

            RaiseCountersChangedEvent();
            _gameEvents.onHintFound.Raise((cell1.Id, cell2.Id));
            _gameManager?.RequestSave();
        }

        private void AddExistingNumbersAsNewLines()
        {
            if (!_actionCountersModel.IsAddNumbersAvailable())
            {
                _gameEvents.onRequestRefillCounters.Raise();
                return;
            }

            if (!_actionCountersModel.AreCountersDisabled)
            {
                _actionCountersModel.DecrementAddNumbers();
            }

            _gridModel.AppendActiveNumbersToGrid();
            _actionHistory.Clear();
            RaiseCountersChangedEvent();
            _gameManager?.RequestSave();
        }

        private void UndoLastAction()
        {
            if (!_actionCountersModel.IsUndoAvailable())
            {
                _gameEvents.onRequestRefillCounters.Raise();
                return;
            }

            if (!_actionHistory.CanUndo()) return;

            _actionHistory.Undo();

            if (!_actionCountersModel.AreCountersDisabled)
            {
                _actionCountersModel.DecrementUndo();
            }

            RaiseCountersChangedEvent();
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));
            _gameManager?.RequestSave();
        }

        private void AttemptMatch((Guid firstCellId, Guid secondCellId) data)
        {
            var firstData = _gridModel.GetCellDataById(data.firstCellId);
            var secondData = _gridModel.GetCellDataById(data.secondCellId);

            if (firstData != null && secondData != null && _matchValidator.IsAValidMatch(firstData, secondData))
            {
                ProcessValidMatch(firstData, secondData);
            }
            else
            {
                _gameEvents.onInvalidMatch.Raise();
            }
        }

        private void ProcessValidMatch(CellData data1, CellData data2)
        {
            var removedLinesInfo = new List<Tuple<int, List<CellData>>>();
            var lineScores = new Dictionary<int, int>();
            var scoreBeforeAction = _statisticsModel.Score;
            var multiplierBeforeAction = _statisticsModel.Multiplier;
            _gameEvents.onMatchFound.Raise((data1.Id, data2.Id));
            _gridModel.SetCellActiveState(data1, false);
            _gridModel.SetCellActiveState(data2, false);
            var pairScore = 1 * _statisticsModel.Multiplier;
            _statisticsModel.AddScore(pairScore);
            _gameEvents.onPairScoreAdded.Raise((data1.Id, data2.Id, pairScore));
            CheckAndRemoveEmptyLines(data1.Line, data2.Line, removedLinesInfo, lineScores);
            if (_gridModel.GetAllActiveCellData().Count == 0)
            {
                _statisticsModel.IncrementMultiplier();
                _gameEvents.onBoardCleared.Raise();
            }

            var action = new MatchAction(data1.Id, data2.Id, removedLinesInfo, scoreBeforeAction, multiplierBeforeAction, pairScore, lineScores);
            _actionHistory.Record(action);
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));
            _gameManager?.RequestSave();
        }

        private int CheckAndRemoveEmptyLines(int line1, int line2, List<Tuple<int, List<CellData>>> removedLinesInfo, Dictionary<int, int> lineScores)
        {
            var linesToRemove = new HashSet<int>();
            if (_gridModel.IsLineEmpty(line1)) linesToRemove.Add(line1);
            if (line1 != line2 && _gridModel.IsLineEmpty(line2)) linesToRemove.Add(line2);
            if (linesToRemove.Count <= 0) return 0;
            foreach (var lineIndex in linesToRemove.OrderByDescending(i => i))
            {
                var lineData = new List<CellData>(_gridModel.Cells[lineIndex]);
                removedLinesInfo.Add(new Tuple<int, List<CellData>>(lineIndex, lineData));
                _gridModel.RemoveLine(lineIndex);
                var scoreForLine = 10 * _statisticsModel.Multiplier;
                _statisticsModel.AddScore(scoreForLine);
                lineScores[lineIndex] = scoreForLine;
                _gameEvents.onLineScoreAdded.Raise((lineIndex, scoreForLine));
            }

            return linesToRemove.Count;
        }

        public void StartNewGame(bool resetStatisticsAndCounters = true)
        {
            _gridModel.ClearField();
            _actionHistory.Clear();

            if (resetStatisticsAndCounters)
            {
                _actionCountersModel.ResetCounters();
                _statisticsModel.Reset();
            }

            RaiseCountersChangedEvent();
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));

            for (var i = 0; i < InitialQuantityByHeight; i++)
            {
                _gridModel.CreateLine(i);
            }

            _gameEvents.onNewGameStarted.Raise();
            _gameManager?.RequestSave();
        }

        ~GameController()
        {
            UnsubscribeFromInputEvents();
        }
    }
}