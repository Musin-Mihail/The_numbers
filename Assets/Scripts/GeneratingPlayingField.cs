using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CellPool))]
public class GeneratingPlayingField : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private ScrollRect scrollRect;

    private readonly Dictionary<Guid, Cell> _cellViewInstances = new();
    private TopLineController _topLineController;
    private CanvasSwiper _canvasSwiper;
    private GridModel _gridModel;
    private CellPool _cellPool;
    private GameInputHandler _inputHandler;
    private int _cellSize;
    private float _scrollLoggingThreshold;
    private float _lastLoggedScrollPosition;
    private GameController _gameController;

    private void Awake()
    {
        _cellPool = GetComponent<CellPool>();
    }

    public void Initialize(GridModel gridModel, TopLineController topLineController, CanvasSwiper canvasSwiper, GameInputHandler inputHandler, GameController gameController)
    {
        _gridModel = gridModel;
        _topLineController = topLineController;
        _canvasSwiper = canvasSwiper;
        _inputHandler = inputHandler;
        _gameController = gameController;

        _gridModel.OnCellAdded += HandleCellAdded;
        _gridModel.OnCellUpdated += HandleCellUpdated;
        _gridModel.OnCellRemoved += HandleCellRemoved;
        _gridModel.OnGridCleared += HandleGridCleared;

        _gameController.OnCellSelected += HandleCellSelected;
        _gameController.OnCellDeselected += HandleCellDeselected;
    }

    private void Start()
    {
        var screenWidth = Screen.width - GameConstants.Indent;
        _cellSize = screenWidth / GameConstants.QuantityByWidth;
        if (cellPrefab)
        {
            cellPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellSize, _cellSize);
        }

        _scrollLoggingThreshold = _cellSize / 2f;

        if (scrollRect)
        {
            _lastLoggedScrollPosition = scrollRect.content.anchoredPosition.y;
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
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

        if (_gameController != null)
        {
            _gameController.OnCellSelected -= HandleCellSelected;
            _gameController.OnCellDeselected -= HandleCellDeselected;
        }
    }

    public void HandleMatchFound(Guid firstCellId, Guid secondCellId)
    {
        UpdateContentSize();
        RefreshTopLine();
    }

    private void HandleCellSelected(Guid cellId)
    {
        if (_cellViewInstances.TryGetValue(cellId, out var cellView))
        {
            cellView.SetSelected(true);
        }
    }

    private void HandleCellDeselected(Guid cellId)
    {
        if (_cellViewInstances.TryGetValue(cellId, out var cellView))
        {
            cellView.SetSelected(false);
        }
    }

    public void HandleInvalidMatch()
    {
        // Здесь можно добавить визуальный/звуковой отклик на неверный выбор
    }

    private void HandleCellAdded(CellData data)
    {
        var newCellView = _cellPool.GetCell();
        newCellView.UpdateFromData(data);
        _inputHandler.SubscribeToCell(newCellView);
        _cellViewInstances[data.Id] = newCellView;

        UpdateCellPosition(data, newCellView);
        UpdateContentSize();
        RefreshTopLine();
        _canvasSwiper?.SwitchToCanvas2();
    }

    private void HandleCellUpdated(CellData data)
    {
        if (_cellViewInstances.TryGetValue(data.Id, out var cellView))
        {
            cellView.UpdateFromData(data);
            UpdateCellPosition(data, cellView);
        }
    }

    private void HandleCellRemoved(Guid dataId)
    {
        if (_cellViewInstances.TryGetValue(dataId, out var cellToReturn))
        {
            _inputHandler.UnsubscribeFromCell(cellToReturn);
            _cellPool.ReturnCell(cellToReturn);
            _cellViewInstances.Remove(dataId);
        }
    }

    private void HandleGridCleared()
    {
        foreach (var cell in _cellViewInstances.Values)
        {
            _inputHandler.UnsubscribeFromCell(cell);
            _cellPool.ReturnCell(cell);
        }

        _cellViewInstances.Clear();
        UpdateContentSize();
    }

    private void UpdateContentSize()
    {
        if (!contentContainer) return;
        var lineCount = _gridModel.Cells.Count;
        var newHeight = lineCount * _cellSize + _cellSize * 1.1f + GameConstants.Indent;
        contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, newHeight);
    }

    private void UpdateCellPosition(CellData data, Cell cellView)
    {
        var targetPosition = new Vector2(
            _cellSize * data.Column + GameConstants.Indent / 2f,
            -_cellSize * data.Line - GameConstants.Indent / 2f - _cellSize * 1.1f
        );

        if (cellView.Animator)
        {
            cellView.Animator.MoveTo(cellView.targetRectTransform, targetPosition, GameConstants.CellMoveDuration);
        }
    }

    private void RefreshTopLine()
    {
        if (!_topLineController) return;
        var numberLine = Mathf.RoundToInt(_lastLoggedScrollPosition / _cellSize);
        var activeNumbers = _gridModel.GetNumbersForTopLine(numberLine);
        _topLineController.UpdateDisplayedNumbers(activeNumbers);
    }

    private void OnScrollValueChanged(Vector2 position)
    {
        var currentScrollPosition = scrollRect.content.anchoredPosition.y;
        if (Mathf.Abs(currentScrollPosition - _lastLoggedScrollPosition) >= _scrollLoggingThreshold)
        {
            _lastLoggedScrollPosition = currentScrollPosition;
            RefreshTopLine();
        }
    }
}