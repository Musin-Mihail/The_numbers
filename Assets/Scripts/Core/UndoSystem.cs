using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace Core
{
    public interface IUndoableAction
    {
        void Undo(GridModel gridModel, StatisticsModel statisticsModel);
    }

    public class MatchAction : IUndoableAction
    {
        private readonly Guid _cell1Id;
        private readonly Guid _cell2Id;
        private readonly List<Tuple<int, List<CellData>>> _removedLines;
        private readonly long _scoreBeforeAction;
        private readonly int _multiplierBeforeAction;

        private readonly int _pairScore;
        private readonly Dictionary<int, int> _lineScores;

        public MatchAction(Guid cell1Id, Guid cell2Id, List<Tuple<int, List<CellData>>> removedLines, long scoreBefore, int multiplierBefore, int pairScore, Dictionary<int, int> lineScores)
        {
            _cell1Id = cell1Id;
            _cell2Id = cell2Id;
            _removedLines = removedLines;
            _scoreBeforeAction = scoreBefore;
            _multiplierBeforeAction = multiplierBefore;
            _pairScore = pairScore;
            _lineScores = lineScores ?? new Dictionary<int, int>();
        }

        public void Undo(GridModel gridModel, StatisticsModel statisticsModel)
        {
            if (_removedLines is { Count: > 0 })
            {
                foreach (var lineInfo in _removedLines.OrderBy(l => l.Item1))
                {
                    gridModel.RestoreLine(lineInfo.Item1, lineInfo.Item2);
                }

                foreach (var lineScore in _lineScores)
                {
                    GameEvents.RaiseLineScoreUndone(lineScore.Key, lineScore.Value);
                }
            }

            var cell1 = gridModel.GetCellDataById(_cell1Id);
            var cell2 = gridModel.GetCellDataById(_cell2Id);
            if (cell1 != null) gridModel.SetCellActiveState(cell1, true);
            if (cell2 != null) gridModel.SetCellActiveState(cell2, true);

            if (_pairScore > 0)
            {
                GameEvents.RaisePairScoreUndone(_cell1Id, _cell2Id, _pairScore);
            }

            statisticsModel.SetState(_scoreBeforeAction, _multiplierBeforeAction);
        }
    }

    public class ActionHistory
    {
        private readonly Stack<IUndoableAction> _actions = new();
        private readonly GridModel _gridModel;

        public ActionHistory(GridModel gridModel)
        {
            _gridModel = gridModel;
        }

        public void Record(IUndoableAction action)
        {
            _actions.Push(action);
        }

        public void Undo(StatisticsModel statisticsModel)
        {
            if (_actions.Count <= 0) return;
            var lastAction = _actions.Pop();
            lastAction.Undo(_gridModel, statisticsModel);
        }

        public bool CanUndo()
        {
            return _actions.Count > 0;
        }

        public void Clear()
        {
            _actions.Clear();
        }
    }
}