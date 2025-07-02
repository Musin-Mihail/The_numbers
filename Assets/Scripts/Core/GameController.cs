using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Model;

namespace Core
{
    public class GameController
    {
        private readonly GridModel _gridModel;
        private readonly MatchValidator _matchValidator;
        private readonly IGridDataProvider _gridDataProvider;
        private readonly ActionHistory _actionHistory;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly StatisticsModel _statisticsModel;
        private bool _countersPermanentlyDisabled;
        private const int InitialQuantityByHeight = 5;

        public GameController(GridModel gridModel, MatchValidator matchValidator, IGridDataProvider gridDataProvider)
        {
            _gridModel = gridModel;
            _matchValidator = matchValidator;
            _gridDataProvider = gridDataProvider;
            _actionHistory = new ActionHistory(_gridModel);
            _actionCountersModel = new ActionCountersModel();
            _statisticsModel = new StatisticsModel();
            _countersPermanentlyDisabled = false;
            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            GameEvents.OnAttemptMatch += AttemptMatch;
            GameEvents.OnAddExistingNumbers += AddExistingNumbersAsNewLines;
            GameEvents.OnUndoLastAction += UndoLastAction;
            GameEvents.OnRequestHint += FindAndShowHint;
            GameEvents.OnRefillCountersConfirmed += HandleRefillCountersConfirmed;
            GameEvents.OnDisableCountersConfirmed += HandleDisableCountersConfirmed;
        }

        private void UnsubscribeFromInputEvents()
        {
            GameEvents.OnAttemptMatch -= AttemptMatch;
            GameEvents.OnAddExistingNumbers -= AddExistingNumbersAsNewLines;
            GameEvents.OnUndoLastAction -= UndoLastAction;
            GameEvents.OnRequestHint -= FindAndShowHint;
            GameEvents.OnRefillCountersConfirmed -= HandleRefillCountersConfirmed;
            GameEvents.OnDisableCountersConfirmed -= HandleDisableCountersConfirmed;
        }

        private void HandleRefillCountersConfirmed()
        {
            _countersPermanentlyDisabled = false;
            _actionCountersModel.ResetCounters();
            RaiseCountersChangedEvent();
        }

        private void RaiseCountersChangedEvent()
        {
            if (_actionCountersModel.AreCountersDisabled)
            {
                GameEvents.RaiseCountersChanged(-1, -1, -1);
            }
            else
            {
                GameEvents.RaiseCountersChanged(_actionCountersModel.UndoCount, _actionCountersModel.AddNumbersCount, _actionCountersModel.HintCount);
            }
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
                GameEvents.RaiseRequestRefillCounters();
                return;
            }

            var activeCells = _gridModel.GetAllActiveCellData();
            if (activeCells.Count < 2)
            {
                GameEvents.RaiseNoHintFound();
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

            GameEvents.RaiseNoHintFound();
        }

        private void ShowHintAndDecrementCounter(CellData cell1, CellData cell2)
        {
            if (!_actionCountersModel.AreCountersDisabled)
            {
                _actionCountersModel.DecrementHint();
            }

            RaiseCountersChangedEvent();
            GameEvents.RaiseHintFound(cell1.Id, cell2.Id);
        }

        private void AddExistingNumbersAsNewLines()
        {
            if (!_actionCountersModel.IsAddNumbersAvailable())
            {
                GameEvents.RaiseRequestRefillCounters();
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
                GameEvents.RaiseRequestRefillCounters();
                return;
            }

            if (!_actionHistory.CanUndo()) return;

            _actionHistory.Undo(_statisticsModel);

            if (!_actionCountersModel.AreCountersDisabled)
            {
                _actionCountersModel.DecrementUndo();
            }

            RaiseCountersChangedEvent();
            GameEvents.RaiseStatisticsChanged(_statisticsModel.Score, _statisticsModel.Multiplier);
        }

        private void AttemptMatch(Guid firstCellId, Guid secondCellId)
        {
            var firstData = _gridModel.GetCellDataById(firstCellId);
            var secondData = _gridModel.GetCellDataById(secondCellId);

            if (firstData != null && secondData != null && _matchValidator.IsAValidMatch(firstData, secondData))
            {
                ProcessValidMatch(firstData, secondData);
            }
            else
            {
                GameEvents.RaiseInvalidMatch();
            }
        }

        private void ProcessValidMatch(CellData data1, CellData data2)
        {
            var removedLinesInfo = new List<Tuple<int, List<CellData>>>();
            var lineScores = new Dictionary<int, int>();
            var scoreBeforeAction = _statisticsModel.Score;
            var multiplierBeforeAction = _statisticsModel.Multiplier;
            GameEvents.RaiseMatchFound(data1.Id, data2.Id);
            _gridModel.SetCellActiveState(data1, false);
            _gridModel.SetCellActiveState(data2, false);
            var pairScore = 1 * _statisticsModel.Multiplier;
            _statisticsModel.AddScore(pairScore);
            GameEvents.RaisePairScoreAdded(data1.Id, data2.Id, pairScore);
            CheckAndRemoveEmptyLines(data1.Line, data2.Line, removedLinesInfo, lineScores);
            if (_gridModel.GetAllActiveCellData().Count == 0)
            {
                _statisticsModel.IncrementMultiplier();
            }

            var action = new MatchAction(data1.Id, data2.Id, removedLinesInfo, scoreBeforeAction, multiplierBeforeAction, pairScore, lineScores);
            _actionHistory.Record(action);
            GameEvents.RaiseStatisticsChanged(_statisticsModel.Score, _statisticsModel.Multiplier);
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
                GameEvents.RaiseLineScoreAdded(lineIndex, scoreForLine);
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
            GameEvents.RaiseStatisticsChanged(_statisticsModel.Score, _statisticsModel.Multiplier);

            for (var i = 0; i < InitialQuantityByHeight; i++)
            {
                _gridModel.CreateLine(i);
            }

            GameEvents.RaiseGameStarted();
        }

        ~GameController()
        {
            UnsubscribeFromInputEvents();
        }
    }
}