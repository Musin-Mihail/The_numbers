using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Gameplay;
using Model;
using UnityEngine;

namespace Core
{
    public class GameController
    {
        private readonly GridModel _gridModel;
        private readonly MatchValidator _matchValidator;
        private readonly ActionHistory _actionHistory;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly StatisticsModel _statisticsModel;
        private bool _countersPermanentlyDisabled;
        private const int InitialQuantityByHeight = 5;
        private readonly GameEvents _gameEvents;

        public GameController(GridModel gridModel, MatchValidator matchValidator, GameEvents gameEvents)
        {
            _gridModel = gridModel;
            _matchValidator = matchValidator;
            _gameEvents = gameEvents;

            _actionHistory = new ActionHistory(_gridModel, _gameEvents.onPairScoreUndone, _gameEvents.onLineScoreUndone);
            _actionCountersModel = new ActionCountersModel();
            _statisticsModel = new StatisticsModel();
            _countersPermanentlyDisabled = false;

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
            _countersPermanentlyDisabled = false;
            _actionCountersModel.ResetCounters();
            RaiseCountersChangedEvent();
        }

        private void RaiseCountersChangedEvent()
        {
            _gameEvents.onCountersChanged.Raise(_actionCountersModel.AreCountersDisabled ? (-1, -1, -1) : (_actionCountersModel.UndoCount, _actionCountersModel.AddNumbersCount, _actionCountersModel.HintCount));
        }

        private void HandleDisableCountersConfirmed()
        {
            _countersPermanentlyDisabled = true;
            _actionCountersModel.DisableCounters();
            RaiseCountersChangedEvent();
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
                Debug.Log("No active cells found");
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

            Debug.Log("No active cells found");

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
        }

        private void UndoLastAction()
        {
            if (!_actionCountersModel.IsUndoAvailable())
            {
                _gameEvents.onRequestRefillCounters.Raise();
                return;
            }

            if (!_actionHistory.CanUndo()) return;

            _actionHistory.Undo(_statisticsModel);

            if (!_actionCountersModel.AreCountersDisabled)
            {
                _actionCountersModel.DecrementUndo();
            }

            RaiseCountersChangedEvent();
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));
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
            }

            var action = new MatchAction(data1.Id, data2.Id, removedLinesInfo, scoreBeforeAction, multiplierBeforeAction, pairScore, lineScores);
            _actionHistory.Record(action);
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));
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

        public void StartNewGame()
        {
            _gridModel.ClearField();
            _actionHistory.Clear();
            _actionCountersModel.ResetCounters();
            _statisticsModel.Reset();

            if (_countersPermanentlyDisabled)
            {
                _actionCountersModel.DisableCounters();
            }

            RaiseCountersChangedEvent();
            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));

            for (var i = 0; i < InitialQuantityByHeight; i++)
            {
                _gridModel.CreateLine(i);
            }

            _gameEvents.onNewGameStarted.Raise();
        }

        ~GameController()
        {
            UnsubscribeFromInputEvents();
        }
    }
}