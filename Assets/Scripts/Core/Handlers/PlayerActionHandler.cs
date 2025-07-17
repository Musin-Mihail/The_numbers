using System;
using Core.Events;
using Model;

namespace Core.Handlers
{
    /// <summary>
    /// Обрабатывает действия игрока, такие как отмена хода и добавление новых чисел.
    /// </summary>
    public class PlayerActionHandler : IDisposable
    {
        private readonly ActionCountersModel _actionCountersModel;
        private readonly ActionHistory _actionHistory;
        private readonly GridModel _gridModel;
        private readonly StatisticsModel _statisticsModel;
        private readonly GameEvents _gameEvents;
        private readonly GameManager _gameManager;

        /// <summary>
        /// Инициализирует обработчик действий игрока с необходимыми зависимостями.
        /// </summary>
        public PlayerActionHandler(
            ActionCountersModel actionCountersModel,
            ActionHistory actionHistory,
            GridModel gridModel,
            StatisticsModel statisticsModel,
            GameEvents gameEvents,
            GameManager gameManager)
        {
            _actionCountersModel = actionCountersModel;
            _actionHistory = actionHistory;
            _gridModel = gridModel;
            _statisticsModel = statisticsModel;
            _gameEvents = gameEvents;
            _gameManager = gameManager;

            _gameEvents.onAddExistingNumbers.AddListener(AddExistingNumbersAsNewLines);
            _gameEvents.onUndoLastAction.AddListener(UndoLastAction);
        }

        /// <summary>
        /// Отписывается от событий.
        /// </summary>
        public void Dispose()
        {
            _gameEvents.onAddExistingNumbers.RemoveListener(AddExistingNumbersAsNewLines);
            _gameEvents.onUndoLastAction.RemoveListener(UndoLastAction);
        }

        /// <summary>
        /// Добавляет существующие на поле числа в виде новых линий.
        /// </summary>
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
            _gameManager?.RequestSave();
        }

        /// <summary>
        /// Отменяет последнее действие игрока.
        /// </summary>
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

            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));
            _gameManager?.RequestSave();
        }
    }
}