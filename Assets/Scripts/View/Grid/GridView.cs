using System;
using System.Collections.Generic;
using Core;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.UI;

namespace View.Grid
{
    [RequireComponent(typeof(CellPool))]
    public class GridView : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform scrollviewContainer;

        [SerializeField] private FloatingScorePool floatingScorePool;
        [SerializeField] private Color positiveScoreColor = Color.green;
        [SerializeField] private Color negativeScoreColor = Color.red;

        private readonly Dictionary<Guid, Cell> _cellViewInstances = new();
        private HeaderNumberDisplay _headerNumberDisplay;
        private GridModel _gridModel;
        private CellPool _cellPool;
        private float _cellSize;
        private float _scrollLoggingThreshold;
        private float _lastLoggedScrollPosition;
        private Guid? _firstSelectedCellId;
        private float _topPaddingValue;
        private readonly List<Guid> _hintedCellIds = new();

        private void Awake()
        {
            _cellPool = GetComponent<CellPool>();
            SubscribeToEvents();
        }

        public void Initialize(GridModel gridModel, HeaderNumberDisplay headerNumberDisplay)
        {
            _gridModel = gridModel;
            _headerNumberDisplay = headerNumberDisplay;

            _gridModel.OnCellAdded += HandleCellAdded;
            _gridModel.OnCellUpdated += HandleCellUpdated;
            _gridModel.OnCellRemoved += HandleCellRemoved;
            _gridModel.OnGridCleared += HandleGridCleared;
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnMatchFound += HandleMatchFound;
            GameEvents.OnInvalidMatch += HandleInvalidMatch;
            GameEvents.OnToggleTopLine += SetTopPaddingActive;
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnHintFound += HandleHintFound;
            GameEvents.OnClearHint += ClearHintVisuals;

            GameEvents.OnPairScoreAdded += HandlePairScoreAdded;
            GameEvents.OnLineScoreAdded += HandleLineScoreAdded;
            GameEvents.OnPairScoreUndone += HandlePairScoreUndone;
            GameEvents.OnLineScoreUndone += HandleLineScoreUndone;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnMatchFound -= HandleMatchFound;
            GameEvents.OnInvalidMatch -= HandleInvalidMatch;
            GameEvents.OnToggleTopLine -= SetTopPaddingActive;
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnHintFound -= HandleHintFound;
            GameEvents.OnClearHint -= ClearHintVisuals;

            GameEvents.OnPairScoreAdded -= HandlePairScoreAdded;
            GameEvents.OnLineScoreAdded -= HandleLineScoreAdded;
            GameEvents.OnPairScoreUndone -= HandlePairScoreUndone;
            GameEvents.OnLineScoreUndone -= HandleLineScoreUndone;
        }

        private void HandleHintFound(Guid firstId, Guid secondId)
        {
            ClearHintVisuals();
            _hintedCellIds.Add(firstId);
            _hintedCellIds.Add(secondId);

            if (_cellViewInstances.TryGetValue(firstId, out var firstCell))
            {
                firstCell.SetHighlight(true);
            }

            if (_cellViewInstances.TryGetValue(secondId, out var secondCell))
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

        private void Start()
        {
            if (!canvasScaler)
            {
                Debug.LogError("Ошибка: CanvasScaler не назначен в инспекторе!", this);
                enabled = false;
                return;
            }

            var referenceWidth = canvasScaler.referenceResolution.x;
            _cellSize = (referenceWidth - GameConstants.Indent) / GameConstants.QuantityByWidth;
            _topPaddingValue = _cellSize * 1.1f;
            if (cellPrefab)
            {
                cellPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellSize, _cellSize);
            }

            _scrollLoggingThreshold = _cellSize / 2f;
            if (!scrollRect) return;
            _lastLoggedScrollPosition = scrollRect.content.anchoredPosition.y;
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
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

        private void OnDestroy()
        {
            if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);

            if (_gridModel != null)
            {
                _gridModel.OnCellAdded -= HandleCellAdded;
                _gridModel.OnCellUpdated -= HandleCellUpdated;
                _gridModel.OnCellRemoved -= HandleCellRemoved;
                _gridModel.OnGridCleared -= HandleGridCleared;
            }

            UnsubscribeFromEvents();
        }

        private void HandleMatchFound(Guid firstCellId, Guid secondCellId)
        {
            GameEvents.RaiseClearHint();
            UpdateContentSize();
            RefreshTopLine();
        }

        private void HandleGameStarted()
        {
            GameEvents.RaiseHideMenu();
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

                    GameEvents.RaiseAttemptMatch(_firstSelectedCellId.Value, clickedCellId);
                    _firstSelectedCellId = null;
                }
            }
        }

        private void HandleInvalidMatch()
        {
            GameEvents.RaiseClearHint();
        }

        private void HandleCellAdded(CellData data, bool animate)
        {
            var newCellView = _cellPool.GetCell();
            newCellView.UpdateFromData(data);
            newCellView.OnClickedCallback = HandleCellClicked;
            _cellViewInstances[data.Id] = newCellView;

            UpdateCellPosition(data, newCellView, animate);
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
            _cellPool.ReturnCell(cellToReturn);
            _cellViewInstances.Remove(dataId);
        }

        private void HandleGridCleared()
        {
            _firstSelectedCellId = null;
            foreach (var cell in _cellViewInstances.Values)
            {
                _cellPool.ReturnCell(cell);
            }

            _cellViewInstances.Clear();
            ClearHintVisuals();
            UpdateContentSize();
        }

        private void UpdateContentSize()
        {
            if (!contentContainer) return;
            var lineCount = _gridModel.Cells.Count;
            var newHeight = lineCount * _cellSize + GameConstants.Indent;
            contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, newHeight);
        }

        private void UpdateCellPosition(CellData data, Cell cellView, bool animate = true)
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

        private void HandlePairScoreAdded(Guid cell1Id, Guid cell2Id, int score)
        {
            ShowFloatingScoreForPair(cell1Id, cell2Id, score, positiveScoreColor);
        }

        private void HandlePairScoreUndone(Guid cell1Id, Guid cell2Id, int score)
        {
            ShowFloatingScoreForPair(cell1Id, cell2Id, -score, negativeScoreColor);
        }

        private void HandleLineScoreAdded(int lineIndex, int score)
        {
            ShowFloatingScoreForLine(lineIndex, score, positiveScoreColor);
        }

        private void HandleLineScoreUndone(int lineIndex, int score)
        {
            ShowFloatingScoreForLine(lineIndex, -score, negativeScoreColor);
        }

        private void ShowFloatingScoreForPair(Guid cell1Id, Guid cell2Id, int score, Color color)
        {
            var cell1Data = _gridModel.GetCellDataById(cell1Id);
            var cell2Data = _gridModel.GetCellDataById(cell2Id);
            if (cell1Data == null || cell2Data == null)
            {
                Debug.LogWarning($"Не удалось найти данные для ячеек {cell1Id} или {cell2Id} для отображения очков.");
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
            scoreTextInstance.Show(score.ToString(), color, position, floatingScorePool.ReturnScore);
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