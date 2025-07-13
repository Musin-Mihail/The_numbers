using System;
using Core.Events;
using Gameplay;
using Model;
using View.Grid;

namespace Core.Handlers
{
    /// <summary>
    /// Обрабатывает логику, связанную с подсказками: поиск допустимой пары и отображение.
    /// </summary>
    public class HintHandler : IDisposable
    {
        private readonly GridModel _gridModel;
        private readonly MatchValidator _matchValidator;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly GameEvents _gameEvents;
        private readonly GridView _gridView;
        private readonly GameManager _gameManager;

        public HintHandler(
            GridModel gridModel,
            MatchValidator matchValidator,
            ActionCountersModel actionCountersModel,
            GameEvents gameEvents,
            GridView gridView,
            GameManager gameManager)
        {
            _gridModel = gridModel;
            _matchValidator = matchValidator;
            _actionCountersModel = actionCountersModel;
            _gameEvents = gameEvents;
            _gridView = gridView;
            _gameManager = gameManager;

            _gameEvents.onRequestHint.AddListener(FindAndShowHint);
        }

        /// <summary>
        /// Отписывается от событий.
        /// </summary>
        public void Dispose()
        {
            _gameEvents.onRequestHint.RemoveListener(FindAndShowHint);
        }

        /// <summary>
        /// Ищет и отображает подсказку (первую найденную допустимую пару).
        /// </summary>
        private void FindAndShowHint()
        {
            if (_gridView.HasActiveHints)
            {
                return;
            }

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

        /// <summary>
        /// Отображает подсказку и уменьшает счетчик.
        /// </summary>
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

        /// <summary>
        /// Вызывает событие изменения счетчиков.
        /// </summary>
        private void RaiseCountersChangedEvent()
        {
            _gameEvents.onCountersChanged.Raise(_actionCountersModel.AreCountersDisabled
                ? (-1, -1, -1)
                : (_actionCountersModel.UndoCount, _actionCountersModel.AddNumbersCount, _actionCountersModel.HintCount));
        }
    }
}