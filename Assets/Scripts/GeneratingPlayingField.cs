using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CellPool))]
public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public RectTransform contentContainer;
    public ScrollRect scrollRect;

    private readonly Dictionary<Guid, Cell> _cellViewInstances = new();
    private TopLineController _topLineController;
    private CanvasSwiper _canvasSwiper;
    private GridModel _gridModel;
    private CellPool _cellPool;
    private GameInputHandler _inputHandler;
    private const int QuantityByWidth = 10;
    private const int Indent = 10;
    private const float MoveDuration = 0.3f;
    private int _screenWidth;
    private int _cellSize;
    private float _scrollLoggingThreshold = 20f;
    private float _lastLoggedScrollPosition;

    private void Awake()
    {
        _cellPool = GetComponent<CellPool>();
        _cellPool.cellPrefab = cellPrefab;
        _cellPool.canvasTransform = contentContainer;
    }

    public void Initialize(GridModel gridModel, TopLineController topLineController, CanvasSwiper canvasSwiper, GameInputHandler inputHandler)
    {
        _gridModel = gridModel;
        _topLineController = topLineController;
        _canvasSwiper = canvasSwiper;
        _inputHandler = inputHandler;
    }

    public Cell GetCellView(Guid dataId)
    {
        _cellViewInstances.TryGetValue(dataId, out var cell);
        return cell;
    }

    public void HandleMatchFound(Cell firstCell, Cell secondCell)
    {
        RefreshTopLine();
    }

    public void HandleInvalidMatch()
    {
        // Этот метод может быть использован для проигрывания звука или визуальных эффектов при неверном выборе.
    }

    public void HandleGridChanged()
    {
        SynchronizeViewWithModel();
        UpdateContentSize();
        UpdateCellsPositions();
        RefreshTopLine();

        if (_canvasSwiper)
        {
            _canvasSwiper.SwitchToCanvas2();
        }
    }

    private void SynchronizeViewWithModel()
    {
        var allDataIds = _gridModel.Cells.SelectMany(line => line).Select(d => d.Id).ToHashSet();

        var idsToRemove = _cellViewInstances.Keys.Where(id => !allDataIds.Contains(id)).ToList();
        foreach (var id in idsToRemove)
        {
            var cellToReturn = _cellViewInstances[id];
            _inputHandler.UnsubscribeFromCell(cellToReturn);
            _cellPool.ReturnCell(cellToReturn);
            _cellViewInstances.Remove(id);
        }

        foreach (var line in _gridModel.Cells)
        {
            foreach (var data in line)
            {
                if (_cellViewInstances.TryGetValue(data.Id, out var cellView))
                {
                    cellView.UpdateFromData(data);
                }
                else
                {
                    var newCellView = _cellPool.GetCell();
                    newCellView.UpdateFromData(data);
                    _inputHandler.SubscribeToCell(newCellView);
                    _cellViewInstances[data.Id] = newCellView;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    private void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    private void Start()
    {
        _screenWidth = Screen.width - Indent;
        _cellSize = _screenWidth / QuantityByWidth;
        _scrollLoggingThreshold = _cellSize / 2;
        if (scrollRect) _lastLoggedScrollPosition = scrollRect.content.anchoredPosition.y;
    }

    private void UpdateContentSize()
    {
        if (!contentContainer) return;
        var lineCount = _gridModel.Cells.Count;
        float newHeight = lineCount * _cellSize + Indent;
        contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, newHeight);
    }

    private void UpdateCellsPositions()
    {
        foreach (var data in _gridModel.Cells.SelectMany(line => line))
        {
            if (_cellViewInstances.TryGetValue(data.Id, out var currentCell))
            {
                var targetPosition = new Vector2(_cellSize * data.Column + Indent / 2, -_cellSize * data.Line - Indent / 2 - _cellSize * 1.1f);
                if (currentCell.Animator)
                {
                    currentCell.Animator.MoveTo(currentCell.targetRectTransform, targetPosition, MoveDuration);
                }
            }
        }
    }

    private void RefreshTopLine()
    {
        if (!_topLineController) return;
        var numberLine = (int)Math.Floor(_lastLoggedScrollPosition / _cellSize);
        var activeNumbers = _gridModel.GetNumbersForTopLine(numberLine, QuantityByWidth);
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