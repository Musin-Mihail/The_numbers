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
        private readonly ActionHistory _actionHistory;
        private const int InitialQuantityByHeight = 5;

        public GameController(GridModel gridModel, MatchValidator matchValidator)
        {
            _gridModel = gridModel;
            _matchValidator = matchValidator;
            _actionHistory = new ActionHistory(_gridModel);

            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            GameEvents.OnAttemptMatch += AttemptMatch;
            GameEvents.OnAddExistingNumbers += AddExistingNumbersAsNewLines;
            GameEvents.OnUndoLastAction += UndoLastAction;
        }

        private void UnsubscribeFromInputEvents()
        {
            GameEvents.OnAttemptMatch -= AttemptMatch;
            GameEvents.OnAddExistingNumbers -= AddExistingNumbersAsNewLines;
            GameEvents.OnUndoLastAction -= UndoLastAction;
        }

        private void AddExistingNumbersAsNewLines()
        {
            _gridModel.AppendActiveNumbersToGrid();
            _actionHistory.Clear();
        }

        private void UndoLastAction()
        {
            _actionHistory.Undo();
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
            GameEvents.RaiseMatchFound(data1.Id, data2.Id);
            _gridModel.SetCellActiveState(data1, false);
            _gridModel.SetCellActiveState(data2, false);
            CheckAndRemoveEmptyLines(data1.Line, data2.Line, removedLinesInfo);
            var action = new MatchAction(data1.Id, data2.Id, removedLinesInfo);
            _actionHistory.Record(action);
        }

        private void CheckAndRemoveEmptyLines(int line1, int line2, List<Tuple<int, List<CellData>>> removedLinesInfo)
        {
            var linesToRemove = new HashSet<int>();
            if (_gridModel.IsLineEmpty(line1)) linesToRemove.Add(line1);
            if (line1 != line2 && _gridModel.IsLineEmpty(line2)) linesToRemove.Add(line2);
            if (linesToRemove.Count <= 0) return;
            foreach (var lineIndex in linesToRemove.OrderByDescending(i => i))
            {
                var lineData = new List<CellData>(_gridModel.Cells[lineIndex]);
                removedLinesInfo.Add(new Tuple<int, List<CellData>>(lineIndex, lineData));
                _gridModel.RemoveLine(lineIndex);
            }
        }

        public void StartNewGame()
        {
            _gridModel.ClearField();
            _actionHistory.Clear();
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