using System;
using System.Collections.Generic;
using Core.Events;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.UI;

namespace View.Grid
{
    /// <summary>
    /// Основной компонент-оркестратор для представления игровой сетки.
    /// Делегирует задачи по обработке ввода, визуализации и расположению
    /// специализированным классам-обработчикам.
    /// </summary>
    public class GridView : MonoBehaviour
    {
        [Header("Зависимости сцены")]
        [SerializeField] private CellPool cellPool;
        [SerializeField] private FloatingScorePool floatingScorePool;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform scrollviewContainer;

        [Header("Настройки")]
        [SerializeField] private Color positiveScoreColor = Color.green;
        [SerializeField] private Color negativeScoreColor = Color.red;

        private readonly Dictionary<Guid, Cell> _cellViewInstances = new();

        private GridModel _gridModel;
        private GameEvents _gameEvents;
        private HeaderNumberDisplay _headerNumberDisplay;

        private GridInputHandler _inputHandler;
        private GridVisuals _visuals;
        private GridLayoutManager _layoutManager;

        /// <summary>
        /// Указывает, есть ли в данный момент активные (подсвеченные) подсказки.
        /// </summary>
        public bool HasActiveHints => _visuals?.HasActiveHints ?? false;

        /// <summary>
        /// Инициализация зависимостей, полученных из GameBootstrap.
        /// </summary>
        public void Initialize(GameEvents gameEvents, GridModel gridModel, HeaderNumberDisplay headerNumberDisplay)
        {
            _gameEvents = gameEvents;
            _gridModel = gridModel;
            _headerNumberDisplay = headerNumberDisplay;
        }

        private void Awake()
        {
            _visuals = new GridVisuals(_cellViewInstances, floatingScorePool, _gridModel, positiveScoreColor, negativeScoreColor);
            _inputHandler = new GridInputHandler(_gameEvents, _visuals);
            _layoutManager = new GridLayoutManager(contentContainer, scrollRect, scrollviewContainer, _headerNumberDisplay, _gridModel);

            _layoutManager.Initialize();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            _layoutManager.Dispose();
        }

        private void SubscribeToEvents()
        {
            if (!_gameEvents) return;

            _gameEvents.onCellAdded.AddListener(HandleCellAdded);
            _gameEvents.onCellUpdated.AddListener(HandleCellUpdated);
            _gameEvents.onCellRemoved.AddListener(HandleCellRemoved);
            _gameEvents.onGridCleared.AddListener(HandleGridCleared);
            _gameEvents.onMatchFound.AddListener(HandleMatchFound);
            _gameEvents.onInvalidMatch.AddListener(HandleInvalidMatch);
            _gameEvents.onToggleTopLine.AddListener(HandleToggleTopLine);
            _gameEvents.onHintFound.AddListener(HandleHintFound);
            _gameEvents.onPairScoreAdded.AddListener(HandlePairScoreAdded);
            _gameEvents.onLineScoreAdded.AddListener(HandleLineScoreAdded);
            _gameEvents.onPairScoreUndone.AddListener(HandlePairScoreUndone);
            _gameEvents.onLineScoreUndone.AddListener(HandleLineScoreUndone);
            _gameEvents.onBoardCleared.AddListener(HandleBoardCleared);
            _gameEvents.onLinesRemoved.AddListener(HandleLinesRemoved);

            _layoutManager.SubscribeToScrollEvents();
        }

        private void UnsubscribeFromEvents()
        {
            if (_gameEvents == null) return;

            _gameEvents.onCellAdded.RemoveListener(HandleCellAdded);
            _gameEvents.onCellUpdated.RemoveListener(HandleCellUpdated);
            _gameEvents.onCellRemoved.RemoveListener(HandleCellRemoved);
            _gameEvents.onGridCleared.RemoveListener(HandleGridCleared);
            _gameEvents.onMatchFound.RemoveListener(HandleMatchFound);
            _gameEvents.onInvalidMatch.RemoveListener(HandleInvalidMatch);
            _gameEvents.onToggleTopLine.RemoveListener(HandleToggleTopLine);
            _gameEvents.onHintFound.RemoveListener(HandleHintFound);
            _gameEvents.onPairScoreAdded.RemoveListener(HandlePairScoreAdded);
            _gameEvents.onLineScoreAdded.RemoveListener(HandleLineScoreAdded);
            _gameEvents.onPairScoreUndone.RemoveListener(HandlePairScoreUndone);
            _gameEvents.onLineScoreUndone.RemoveListener(HandleLineScoreUndone);
            _gameEvents.onBoardCleared.RemoveListener(HandleBoardCleared);
            _gameEvents.onLinesRemoved.RemoveListener(HandleLinesRemoved);

            _layoutManager.UnsubscribeFromScrollEvents();
        }

        /// <summary>
        /// Сбрасывает выделение ячеек и визуальные эффекты подсказок.
        /// </summary>
        public void ResetSelectionAndHints()
        {
            _inputHandler.ResetSelection();
            _visuals.ClearHintVisuals();
        }

        /// <summary>
        /// Полностью перерисовывает сетку на основе текущего состояния GridModel.
        /// </summary>
        public void FullRedraw()
        {
            HandleGridCleared();
            var allCells = _gridModel.GetAllCellData();
            foreach (var cellData in allCells)
            {
                HandleCellAdded((cellData, false));
            }

            _layoutManager.UpdateContentSize();
            _layoutManager.RefreshTopLine();
        }

        private void HandleCellAdded((CellData data, bool animate) payload)
        {
            if (_cellViewInstances.ContainsKey(payload.data.Id)) return;

            var newCellView = cellPool.GetCell();
            newCellView.UpdateFromData(payload.data);
            newCellView.OnClickedCallback = _inputHandler.HandleCellClicked;
            _cellViewInstances[payload.data.Id] = newCellView;

            _layoutManager.UpdateCellPosition(payload.data, newCellView, payload.animate);
            _layoutManager.UpdateContentSize();
            _layoutManager.RefreshTopLine();
        }

        private void HandleCellUpdated(CellData data)
        {
            if (!_cellViewInstances.TryGetValue(data.Id, out var cellView)) return;
            cellView.UpdateFromData(data);
            _layoutManager.UpdateCellPosition(data, cellView, true);
        }

        private void HandleCellRemoved(Guid dataId)
        {
            if (!_cellViewInstances.TryGetValue(dataId, out var cellToReturn)) return;
            cellPool.ReturnCell(cellToReturn);
            _cellViewInstances.Remove(dataId);
        }

        private void HandleGridCleared()
        {
            _inputHandler.ResetSelection();
            _visuals.ClearHintVisuals();

            foreach (var cell in _cellViewInstances.Values)
            {
                cellPool.ReturnCell(cell);
            }

            _cellViewInstances.Clear();
            _layoutManager.UpdateContentSize();
        }

        private void HandleLinesRemoved()
        {
            _layoutManager.UpdateContentSize();
            _layoutManager.RefreshTopLine();
        }

        private void HandleBoardCleared()
        {
            _visuals.ShowBoardClearedMessage(scrollRect.viewport);
        }

        private void HandleHintFound((Guid firstId, Guid secondId) data)
        {
            _visuals.ShowHint(data);
        }

        private void HandleMatchFound((Guid firstCellId, Guid secondCellId) data)
        {
            _visuals.ClearHintVisuals();
        }

        private void HandleInvalidMatch()
        {
            _visuals.ClearHintVisuals();
        }

        private void HandleToggleTopLine(bool isActive)
        {
            _layoutManager.SetTopPaddingActive(isActive);
        }

        private void HandlePairScoreAdded((Guid cell1, Guid cell2, int score) data)
        {
            _visuals.ShowFloatingScoreForPair(data.cell1, data.cell2, data.score, true);
        }

        private void HandlePairScoreUndone((Guid cell1, Guid cell2, int score) data)
        {
            _visuals.ShowFloatingScoreForPair(data.cell1, data.cell2, -data.score, false);
        }

        private void HandleLineScoreAdded((int lineIndex, int score) data)
        {
            _visuals.ShowFloatingScoreForLine(data.lineIndex, data.score, true);
        }

        private void HandleLineScoreUndone((int lineIndex, int score) data)
        {
            _visuals.ShowFloatingScoreForLine(data.lineIndex, -data.score, false);
        }
    }
}