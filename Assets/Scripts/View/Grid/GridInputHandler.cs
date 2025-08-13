using System;
using Core.Events;

namespace View.Grid
{
    /// <summary>
    /// Обрабатывает ввод пользователя, связанный с игровой сеткой,
    /// такой как клики по ячейкам и формирование пар.
    /// </summary>
    public class GridInputHandler
    {
        private readonly GameEvents _gameEvents;
        private readonly GridVisuals _gridVisuals;

        private Guid? _firstSelectedCellId;

        public GridInputHandler(GameEvents gameEvents, GridVisuals gridVisuals)
        {
            _gameEvents = gameEvents;
            _gridVisuals = gridVisuals;
        }

        public bool IsCellSelected(Guid id) => _firstSelectedCellId.HasValue && _firstSelectedCellId.Value == id;

        /// <summary>
        /// Обрабатывает клик по ячейке.
        /// </summary>
        /// <param name="clickedCellId">ID ячейки, по которой кликнули.</param>
        public void HandleCellClicked(Guid clickedCellId)
        {
            if (!_firstSelectedCellId.HasValue)
            {
                _firstSelectedCellId = clickedCellId;
                _gridVisuals.SetSelectionVisual(clickedCellId, true);
            }
            else
            {
                if (_firstSelectedCellId.Value == clickedCellId)
                {
                    _gridVisuals.SetSelectionVisual(clickedCellId, false);
                    _firstSelectedCellId = null;
                }
                else
                {
                    _gridVisuals.SetSelectionVisual(_firstSelectedCellId.Value, false);
                    _gameEvents.onAttemptMatch.Raise((_firstSelectedCellId.Value, clickedCellId));
                    _firstSelectedCellId = null;
                }
            }
        }

        /// <summary>
        /// Сбрасывает текущее состояние выбора.
        /// </summary>
        public void ResetSelection()
        {
            if (!_firstSelectedCellId.HasValue) return;
            _gridVisuals.SetSelectionVisual(_firstSelectedCellId.Value, false);
            _firstSelectedCellId = null;
        }
    }
}