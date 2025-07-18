using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Interfaces;
using Model;

namespace Core.UndoSystem
{
    /// <summary>
    /// Представляет собой действие совпадения пары чисел, которое можно отменить.
    /// </summary>
    public class MatchAction : IUndoableAction
    {
        private readonly Guid _cell1Id;
        private readonly Guid _cell2Id;
        private readonly List<Tuple<int, List<CellData>>> _removedLines;
        private readonly long _scoreBeforeAction;
        private readonly int _multiplierBeforeAction;
        private readonly int _pairScore;
        private readonly Dictionary<int, int> _lineScores;

        private readonly GridModel _gridModel;
        private readonly StatisticsModel _statisticsModel;
        private readonly GameEvents _gameEvents;

        /// <summary>
        /// Инициализирует новое отменяемое действие совпадения пары.
        /// </summary>
        public MatchAction(Guid cell1Id, Guid cell2Id, List<Tuple<int, List<CellData>>> removedLines, long scoreBefore, int multiplierBefore, int pairScore, Dictionary<int, int> lineScores, GridModel gridModel, StatisticsModel statisticsModel, GameEvents gameEvents)
        {
            _cell1Id = cell1Id;
            _cell2Id = cell2Id;
            _removedLines = removedLines;
            _scoreBeforeAction = scoreBefore;
            _multiplierBeforeAction = multiplierBefore;
            _pairScore = pairScore;
            _lineScores = lineScores ?? new Dictionary<int, int>();

            _gridModel = gridModel;
            _statisticsModel = statisticsModel;
            _gameEvents = gameEvents;
        }

        /// <summary>
        /// Отменяет совпадение, восстанавливая ячейки, линии и статистику.
        /// </summary>
        public void Undo()
        {
            if (_removedLines is { Count: > 0 })
            {
                foreach (var lineInfo in _removedLines.OrderBy(l => l.Item1))
                {
                    _gridModel.RestoreLine(lineInfo.Item1, lineInfo.Item2);
                }

                foreach (var lineScore in _lineScores)
                {
                    _gameEvents.onLineScoreUndone.Raise((lineScore.Key, lineScore.Value));
                }
            }

            var cell1 = _gridModel.GetCellDataById(_cell1Id);
            var cell2 = _gridModel.GetCellDataById(_cell2Id);
            if (cell1 != null) _gridModel.SetCellActiveState(cell1, true);
            if (cell2 != null) _gridModel.SetCellActiveState(cell2, true);

            if (_pairScore > 0)
            {
                _gameEvents.onPairScoreUndone.Raise((_cell1Id, _cell2Id, _pairScore));
            }

            _statisticsModel.SetState(_scoreBeforeAction, _multiplierBeforeAction);
        }
    }
}