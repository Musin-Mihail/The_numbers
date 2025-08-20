using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.UI;

namespace View.Grid
{
    /// <summary>
    /// Основной компонент-оркестратор для представления игровой сетки.
    /// Реализует виртуализацию UI, отображая только видимые ячейки.
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
        [Tooltip("Количество рядов ячеек, которые будут созданы за пределами видимой области для плавной прокрутки.")]
        [SerializeField] private int lineBuffer = 2;

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
            _visuals = new GridVisuals(_cellViewInstances, floatingScorePool, positiveScoreColor, negativeScoreColor);
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
            _gameEvents.onNewGameStarted.AddListener(FullRedraw);
            _gameEvents.onCellAdded.AddListener(HandleGridChanged);
            _gameEvents.onCellUpdated.AddListener(HandleCellUpdated);
            _gameEvents.onCellRemoved.AddListener(HandleGridChanged);
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
            _gameEvents.onLinesRemoved.AddListener(HandleGridChanged);

            scrollRect.onValueChanged.AddListener(OnScrollPositionChanged);
        }

        private void UnsubscribeFromEvents()
        {
            if (!_gameEvents) return;

            _gameEvents.onNewGameStarted.RemoveListener(FullRedraw);
            _gameEvents.onCellAdded.RemoveListener(HandleGridChanged);
            _gameEvents.onCellUpdated.RemoveListener(HandleCellUpdated);
            _gameEvents.onCellRemoved.RemoveListener(HandleGridChanged);
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
            _gameEvents.onLinesRemoved.RemoveListener(HandleGridChanged);

            scrollRect.onValueChanged.RemoveListener(OnScrollPositionChanged);
        }

        private void OnScrollPositionChanged(Vector2 pos)
        {
            UpdateVisibleCells();
            _layoutManager.RefreshTopLine();
        }

        /// <summary>
        /// Основной метод виртуализации. Создает/удаляет вьюшки ячеек на основе их видимости.
        /// </summary>
        private void UpdateVisibleCells()
        {
            if (_gridModel == null || !scrollRect) return;
            // 1. Рассчитать диапазон видимых линий
            var (firstVisibleLine, lastVisibleLine) = _layoutManager.GetVisibleLineRange(lineBuffer);
            // 2. Определить, какие ячейки должны быть видимы
            var requiredCells = new HashSet<Guid>();
            for (var i = firstVisibleLine; i <= lastVisibleLine; i++)
            {
                if (i < 0 || i >= _gridModel.Cells.Count) continue;
                foreach (var cellData in _gridModel.Cells[i])
                {
                    requiredCells.Add(cellData.Id);
                }
            }

            // 3. Удалить ячейки, которые больше не видны
            var cellsToRemove = _cellViewInstances.Keys.Where(cellId => !requiredCells.Contains(cellId)).ToList();
            foreach (var cellId in cellsToRemove)
            {
                if (!_cellViewInstances.TryGetValue(cellId, out var cellView)) continue;
                // Перед удалением сбрасываем состояние, чтобы не было "глюков" при переиспользовании
                _visuals.ClearCellVisuals(cellView);
                cellPool.ReturnCell(cellView);
                _cellViewInstances.Remove(cellId);
            }

            // 4. Добавить ячейки, которые стали видимыми
            for (var i = firstVisibleLine; i <= lastVisibleLine; i++)
            {
                if (i < 0 || i >= _gridModel.Cells.Count) continue;
                foreach (var cellData in _gridModel.Cells[i])
                {
                    if (_cellViewInstances.ContainsKey(cellData.Id)) continue;
                    var newCellView = cellPool.GetCell();
                    newCellView.UpdateFromData(cellData);
                    newCellView.OnClickedCallback = _inputHandler.HandleCellClicked;
                    // Применяем сохраненные состояния (выделение, подсказка)
                    newCellView.SetSelected(_inputHandler.IsCellSelected(cellData.Id));
                    newCellView.SetHighlight(_visuals.IsCellHinted(cellData.Id));
                    _cellViewInstances[cellData.Id] = newCellView;
                    _layoutManager.UpdateCellPosition(cellData, newCellView, false); // Без анимации при прокрутке
                }
            }
        }

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
            _layoutManager.UpdateContentSize();
            UpdateVisibleCells();
            _layoutManager.RefreshTopLine();
        }

        private void HandleGridChanged<T>(T payload) => HandleGridChanged();

        private void HandleGridChanged()
        {
            _layoutManager.UpdateContentSize();
            UpdateVisibleCells();
        }

        private void HandleCellUpdated(CellData data)
        {
            if (!_cellViewInstances.TryGetValue(data.Id, out var cellView)) return;
            cellView.UpdateFromData(data);
            _layoutManager.UpdateCellPosition(data, cellView, true);
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
            var pos1 = _layoutManager.GetCellPosition(data.cell1);
            var pos2 = _layoutManager.GetCellPosition(data.cell2);
            if (pos1.HasValue && pos2.HasValue)
            {
                _visuals.ShowFloatingScoreForPair(pos1.Value, pos2.Value, data.score, true);
            }
        }

        private void HandlePairScoreUndone((Guid cell1, Guid cell2, int score) data)
        {
            var pos1 = _layoutManager.GetCellPosition(data.cell1);
            var pos2 = _layoutManager.GetCellPosition(data.cell2);
            if (pos1.HasValue && pos2.HasValue)
            {
                _visuals.ShowFloatingScoreForPair(pos1.Value, pos2.Value, -data.score, false);
            }
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
