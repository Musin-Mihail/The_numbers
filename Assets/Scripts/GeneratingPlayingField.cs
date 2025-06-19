using System;
using System.Collections.Generic;
using System.Linq;
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
    private int _screenWidth;

    private int _cellSize;
    private float _scrollLoggingThreshold;
    private float _lastLoggedScrollPosition;

    private void Awake()
    {
        _cellPool = GetComponent<CellPool>();
    }

    public void Initialize(GridModel gridModel, TopLineController topLineController, CanvasSwiper canvasSwiper, GameInputHandler inputHandler)
    {
        _gridModel = gridModel;
        _topLineController = topLineController;
        _canvasSwiper = canvasSwiper;
        _inputHandler = inputHandler;
    }

    private void Start()
    {
        var screenWidth = Screen.width - GameConstants.Indent;
        _cellSize = screenWidth / GameConstants.QuantityByWidth;
        cellPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellSize, _cellSize);

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
        // Здесь можно добавить визуальный/звуковой отклик на неверный выбор
    }

    public void HandleGridChanged()
    {
        SynchronizeViewWithModel();
        UpdateContentSize();
        UpdateCellsPositions();
        RefreshTopLine();
        _canvasSwiper?.SwitchToCanvas2();
    }

    private void SynchronizeViewWithModel()
    {
        var allDataIds = _gridModel.GetAllCellData().Select(d => d.Id).ToHashSet();
        var viewIds = _cellViewInstances.Keys.ToList();

        foreach (var id in viewIds)
        {
            if (!allDataIds.Contains(id))
            {
                var cellToReturn = _cellViewInstances[id];
                _inputHandler.UnsubscribeFromCell(cellToReturn);
                _cellPool.ReturnCell(cellToReturn);
                _cellViewInstances.Remove(id);
            }
        }

        foreach (var data in _gridModel.GetAllCellData())
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

    private void UpdateContentSize()
    {
        if (!contentContainer) return;
        var lineCount = _gridModel.Cells.Count;
        float newHeight = lineCount * _cellSize + GameConstants.Indent;
        contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, newHeight);
    }

    private void UpdateCellsPositions()
    {
        foreach (var data in _gridModel.GetAllCellData())
        {
            if (_cellViewInstances.TryGetValue(data.Id, out var currentCell))
            {
                var targetPosition = new Vector2(
                    _cellSize * data.Column + GameConstants.Indent / 2f,
                    -_cellSize * data.Line - GameConstants.Indent / 2f - _cellSize * 1.1f
                );

                if (currentCell.Animator != null)
                {
                    currentCell.Animator.MoveTo(currentCell.targetRectTransform, targetPosition, GameConstants.CellMoveDuration);
                }
            }
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