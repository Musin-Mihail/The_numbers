using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CellPool))]
public class GeneratingPlayingField : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private ScrollRect scrollRect;

    private readonly Dictionary<Guid, Cell> _cellViewInstances = new();
    private TopLineController _topLineController;
    private WindowSwiper _windowSwiper;
    private GridModel _gridModel;
    private CellPool _cellPool;
    private float _cellSize;
    private float _scrollLoggingThreshold;
    private float _lastLoggedScrollPosition;
    private GameController _gameController;
    private Guid? _firstSelectedCellId;

    private void Awake()
    {
        _cellPool = GetComponent<CellPool>();
    }

    public void Initialize(GridModel gridModel, TopLineController topLineController, WindowSwiper windowSwiper, GameController gameController)
    {
        _gridModel = gridModel;
        _topLineController = topLineController;
        _windowSwiper = windowSwiper;
        _gameController = gameController;

        _gridModel.OnCellAdded += HandleCellAdded;
        _gridModel.OnCellUpdated += HandleCellUpdated;
        _gridModel.OnCellRemoved += HandleCellRemoved;
        _gridModel.OnGridCleared += HandleGridCleared;
    }

    private void RefreshTopLine()
    {
        if (!_topLineController) return;
        var numberLine = Mathf.RoundToInt(_lastLoggedScrollPosition / _cellSize);
        var activeNumbers = _gridModel.GetNumbersForTopLine(numberLine);
        _topLineController.UpdateDisplayedNumbers(activeNumbers);
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
    }

    public void HandleMatchFound(Guid firstCellId, Guid secondCellId)
    {
        UpdateContentSize();
        RefreshTopLine();
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

                _gameController.AttemptMatch(_firstSelectedCellId.Value, clickedCellId);
                _firstSelectedCellId = null;
            }
        }
    }

    public void HandleInvalidMatch()
    {
    }

    private void HandleCellAdded(CellData data)
    {
        var newCellView = _cellPool.GetCell();
        newCellView.UpdateFromData(data);
        newCellView.OnClickedCallback = HandleCellClicked;
        _cellViewInstances[data.Id] = newCellView;

        UpdateCellPosition(data, newCellView);
        UpdateContentSize();
        RefreshTopLine();
        _windowSwiper?.SwitchToWindowGame();
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
            _cellPool.ReturnCell(cellToReturn);
            _cellViewInstances.Remove(dataId);
        }
    }

    private void HandleGridCleared()
    {
        _firstSelectedCellId = null;
        foreach (var cell in _cellViewInstances.Values)
        {
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