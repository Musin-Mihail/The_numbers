using System;
using System.Collections.Generic;
using Core;
using Core.Events;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.UI;

namespace View.Grid
{
    /// <summary>
    /// Основной компонент представления, отвечающий за отображение игровой сетки.
    /// Управляет созданием, удалением и обновлением визуальных представлений ячеек (Cell).
    /// </summary>
    public class GridView : MonoBehaviour
    {
        [Header("Зависимости сцены")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform scrollviewContainer;
        [SerializeField] private FloatingScorePool floatingScorePool;
        [SerializeField] private CellPool cellPool;

        [Header("Настройки")]
        [SerializeField] private Color positiveScoreColor = Color.green;
        [SerializeField] private Color negativeScoreColor = Color.red;

        private readonly Dictionary<Guid, Cell> _cellViewInstances = new();
        private HeaderNumberDisplay _headerNumberDisplay;
        private GridModel _gridModel;
        private GameEvents _gameEvents;

        private float _cellSize;
        private float _scrollLoggingThreshold;
        private float _lastLoggedScrollPosition;
        private Guid? _firstSelectedCellId;
        private float _topPaddingValue;
        private readonly List<Guid> _hintedCellIds = new();

        /// <summary>
        /// Указывает, есть ли в данный момент активные (подсвеченные) подсказки.
        /// </summary>
        public bool HasActiveHints => _hintedCellIds.Count > 0;

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
            if (!cellPrefab)
            {
                Debug.LogError("Ошибка: cellPrefab не назначен в инспекторе!", this);
                enabled = false;
                return;
            }

            _cellSize = cellPrefab.GetComponent<RectTransform>().sizeDelta.x;
            _topPaddingValue = _cellSize * 1.1f;
            _scrollLoggingThreshold = _cellSize / 2f;

            if (!scrollRect) return;
            _lastLoggedScrollPosition = scrollRect.content.anchoredPosition.y;
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
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
            if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
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
            _gameEvents.onToggleTopLine.AddListener(SetTopPaddingActive);
            _gameEvents.onNewGameStarted.AddListener(HandleGameStarted);
            _gameEvents.onHintFound.AddListener(HandleHintFound);
            _gameEvents.onPairScoreAdded.AddListener(HandlePairScoreAdded);
            _gameEvents.onLineScoreAdded.AddListener(HandleLineScoreAdded);
            _gameEvents.onPairScoreUndone.AddListener(HandlePairScoreUndone);
            _gameEvents.onLineScoreUndone.AddListener(HandleLineScoreUndone);
            _gameEvents.onBoardCleared.AddListener(HandleBoardCleared);
            _gameEvents.onLinesRemoved.AddListener(HandleLinesRemoved);
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
            _gameEvents.onToggleTopLine.RemoveListener(SetTopPaddingActive);
            _gameEvents.onNewGameStarted.RemoveListener(HandleGameStarted);
            _gameEvents.onHintFound.RemoveListener(HandleHintFound);
            _gameEvents.onPairScoreAdded.RemoveListener(HandlePairScoreAdded);
            _gameEvents.onLineScoreAdded.RemoveListener(HandleLineScoreAdded);
            _gameEvents.onPairScoreUndone.RemoveListener(HandlePairScoreUndone);
            _gameEvents.onLineScoreUndone.RemoveListener(HandleLineScoreUndone);
            _gameEvents.onBoardCleared.RemoveListener(HandleBoardCleared);
            _gameEvents.onLinesRemoved.RemoveListener(HandleLinesRemoved);
        }

        /// <summary>
        /// Сбрасывает выделение ячеек и визуальные эффекты подсказок.
        /// </summary>
        public void ResetSelectionAndHints()
        {
            ClearHintVisuals();
            if (!_firstSelectedCellId.HasValue) return;

            if (_cellViewInstances.TryGetValue(_firstSelectedCellId.Value, out var firstCellView))
            {
                firstCellView.SetSelected(false);
            }

            _firstSelectedCellId = null;
        }

        /// <summary>
        /// Обрабатывает событие очистки доски, показывая уведомление о множителе.
        /// </summary>
        private void HandleBoardCleared()
        {
            if (floatingScorePool == null) return;
            var centerPosition = new Vector2(scrollRect.viewport.rect.width / 2f, -scrollRect.viewport.rect.height / 2f);
            var size = new Vector2(700, 250);
            var adjustedPosition = new Vector2(centerPosition.x - size.x / 2f, centerPosition.y + size.y / 2f);
            var scoreTextInstance = floatingScorePool.GetScore();
            scoreTextInstance.Show("Множитель +1", positiveScoreColor, adjustedPosition, size, floatingScorePool.ReturnScore);
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

            UpdateContentSize();
            RefreshTopLine();
        }

        /// <summary>
        /// Обработчик события удаления линий. Обновляет размер контейнера.
        /// </summary>
        private void HandleLinesRemoved()
        {
            UpdateContentSize();
            RefreshTopLine();
        }

        private void HandleHintFound((Guid firstId, Guid secondId) data)
        {
            ClearHintVisuals();
            _hintedCellIds.Add(data.firstId);
            _hintedCellIds.Add(data.secondId);

            if (_cellViewInstances.TryGetValue(data.firstId, out var firstCell))
            {
                firstCell.SetHighlight(true);
            }

            if (_cellViewInstances.TryGetValue(data.secondId, out var secondCell))
            {
                secondCell.SetHighlight(true);
            }
        }

        private void ClearHintVisuals()
        {
            if (_hintedCellIds.Count == 0) return;

            foreach (var id in _hintedCellIds)
            {
                if (_cellViewInstances.TryGetValue(id, out var cell))
                {
                    cell.SetHighlight(false);
                }
            }

            _hintedCellIds.Clear();
        }

        private void RefreshTopLine()
        {
            if (!_headerNumberDisplay) return;
            var numberLine = Mathf.RoundToInt(_lastLoggedScrollPosition / _cellSize);
            var activeNumbers = _gridModel.GetNumbersForTopLine(numberLine);
            _headerNumberDisplay.UpdateDisplayedNumbers(activeNumbers);
        }

        private void SetTopPaddingActive(bool isActive)
        {
            if (scrollviewContainer)
            {
                var topOffset = isActive ? -_topPaddingValue : 0;
                scrollviewContainer.offsetMax = new Vector2(scrollviewContainer.offsetMax.x, topOffset);
            }
            else
            {
                Debug.LogWarning("scrollviewContainer не назначен в инспекторе GridView. Отступ сверху не будет применен.");
            }
        }

        private void HandleMatchFound((Guid firstCellId, Guid secondCellId) data)
        {
            ClearHintVisuals();
        }

        private void HandleGameStarted()
        {
            // Логика, которая должна выполняться при старте новой игры
        }

        private void HandleCellClicked(Guid clickedCellId)
        {
            if (!_firstSelectedCellId.HasValue)
            {
                _firstSelectedCellId = clickedCellId;
                if (_cellViewInstances.TryGetValue(clickedCellId, out var cellView))
                {
                    cellView.SetSelected(true);
                }
            }
            else
            {
                if (_firstSelectedCellId.Value == clickedCellId)
                {
                    if (_cellViewInstances.TryGetValue(clickedCellId, out var cellView))
                    {
                        cellView.SetSelected(false);
                    }

                    _firstSelectedCellId = null;
                }
                else
                {
                    if (_cellViewInstances.TryGetValue(_firstSelectedCellId.Value, out var firstCellView))
                    {
                        firstCellView.SetSelected(false);
                    }

                    _gameEvents.onAttemptMatch.Raise((_firstSelectedCellId.Value, clickedCellId));
                    _firstSelectedCellId = null;
                }
            }
        }

        private void HandleInvalidMatch()
        {
            ClearHintVisuals();
        }

        private void HandleCellAdded((CellData data, bool animate) payload)
        {
            if (_cellViewInstances.ContainsKey(payload.data.Id)) return;

            var newCellView = cellPool.GetCell();
            newCellView.UpdateFromData(payload.data);
            newCellView.OnClickedCallback = HandleCellClicked;
            _cellViewInstances[payload.data.Id] = newCellView;

            UpdateCellPosition(payload.data, newCellView, payload.animate);
            UpdateContentSize();
            RefreshTopLine();
        }

        private void HandleCellUpdated(CellData data)
        {
            if (!_cellViewInstances.TryGetValue(data.Id, out var cellView)) return;
            cellView.UpdateFromData(data);
            UpdateCellPosition(data, cellView, true);
        }

        private void HandleCellRemoved(Guid dataId)
        {
            if (!_cellViewInstances.TryGetValue(dataId, out var cellToReturn)) return;
            cellPool.ReturnCell(cellToReturn);
            _cellViewInstances.Remove(dataId);
        }

        private void HandleGridCleared()
        {
            _firstSelectedCellId = null;
            foreach (var cell in _cellViewInstances.Values)
            {
                cellPool.ReturnCell(cell);
            }

            _cellViewInstances.Clear();
            _hintedCellIds.Clear();
            UpdateContentSize();
        }

        private void UpdateContentSize()
        {
            if (!contentContainer) return;
            var lineCount = _gridModel.Cells.Count;
            var newHeight = lineCount * _cellSize + GameConstants.Indent;
            contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, newHeight);
        }

        private void UpdateCellPosition(CellData data, Cell cellView, bool animate)
        {
            var targetPosition = GetCellPosition(data);

            if (!cellView.Animator) return;
            var duration = animate ? GameConstants.CellMoveDuration : 0f;
            cellView.Animator.MoveTo(cellView.TargetRectTransform, targetPosition, duration);
        }

        private void OnScrollValueChanged(Vector2 position)
        {
            var currentScrollPosition = scrollRect.content.anchoredPosition.y;
            if (!(Mathf.Abs(currentScrollPosition - _lastLoggedScrollPosition) >= _scrollLoggingThreshold)) return;
            _lastLoggedScrollPosition = currentScrollPosition;
            RefreshTopLine();
        }

        private void HandlePairScoreAdded((Guid cell1, Guid cell2, int score) data)
        {
            ShowFloatingScoreForPair(data.cell1, data.cell2, data.score, positiveScoreColor);
        }

        private void HandlePairScoreUndone((Guid cell1, Guid cell2, int score) data)
        {
            ShowFloatingScoreForPair(data.cell1, data.cell2, -data.score, negativeScoreColor);
        }

        private void HandleLineScoreAdded((int lineIndex, int score) data)
        {
            ShowFloatingScoreForLine(data.lineIndex, data.score, positiveScoreColor);
        }

        private void HandleLineScoreUndone((int lineIndex, int score) data)
        {
            ShowFloatingScoreForLine(data.lineIndex, -data.score, negativeScoreColor);
        }

        private void ShowFloatingScoreForPair(Guid cell1Id, Guid cell2Id, int score, Color color)
        {
            var cell1Data = _gridModel.GetCellDataById(cell1Id);
            var cell2Data = _gridModel.GetCellDataById(cell2Id);
            if (cell1Data == null || cell2Data == null)
            {
                return;
            }

            var pos1Pivot = GetCellPosition(cell1Data);
            var pos2Pivot = GetCellPosition(cell2Data);
            var midPoint = (pos1Pivot + pos2Pivot) / 2f;
            ShowFloatingScore(score, color, midPoint);
        }

        private void ShowFloatingScoreForLine(int lineIndex, int score, Color color)
        {
            var yPos = -lineIndex * _cellSize - (_cellSize / 2f) - (GameConstants.Indent / 2f);
            var xPos = contentContainer.rect.width / 2f;
            var position = new Vector2(xPos, yPos);
            ShowFloatingScore(score, color, position);
        }

        private void ShowFloatingScore(int score, Color color, Vector2 position)
        {
            if (floatingScorePool == null) return;
            var scoreTextInstance = floatingScorePool.GetScore();
            scoreTextInstance.Show(Mathf.Abs(score).ToString(), color, position, new Vector2(107, 107), floatingScorePool.ReturnScore);
        }

        private Vector2 GetCellPosition(CellData data)
        {
            return new Vector2(
                _cellSize * data.Column + GameConstants.Indent / 2f,
                -_cellSize * data.Line - GameConstants.Indent / 2f
            );
        }
    }
}