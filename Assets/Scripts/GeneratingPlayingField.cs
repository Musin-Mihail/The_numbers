using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CellPool))]
public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public RectTransform contentContainer;
    public ScrollRect scrollRect;

    private float _scrollLoggingThreshold = 20f;
    private TopLineController _topLineController;
    private CanvasSwiper _canvasSwiper;
    private GridModel _gridModel;
    private CellPool _cellPool;

    private const int QuantityByWidth = 10;
    private const int Indent = 10;
    private const float MoveDuration = 0.3f;
    private int _screenWidth;
    private int _cellSize;
    private bool _isAnimating;
    private float _lastLoggedScrollPosition;

    private void Awake()
    {
        _cellPool = GetComponent<CellPool>();
        _cellPool.cellPrefab = cellPrefab;
        _cellPool.canvasTransform = contentContainer;
    }

    public void Initialize(GridModel gridModel, TopLineController topLineController, CanvasSwiper canvasSwiper)
    {
        _gridModel = gridModel;
        _topLineController = topLineController;
        _canvasSwiper = canvasSwiper;
    }

    private void OnEnable()
    {
        if (scrollRect)
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }

    private void OnDisable()
    {
        if (scrollRect)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
    }

    private void Start()
    {
        _screenWidth = Screen.width - Indent;
        _cellSize = _screenWidth / QuantityByWidth;
        _scrollLoggingThreshold = _cellSize / 2;

        if (scrollRect)
        {
            _lastLoggedScrollPosition = scrollRect.content.anchoredPosition.y;
        }
    }

    public void HandleMatchFound(Cell firstCell, Cell secondCell)
    {
        RefreshTopLine();
    }

    public void HandleInvalidMatch()
    {
        foreach (var cell in _gridModel.GetAllActiveCells())
        {
            cell.SetSelected(false);
        }
    }

    public void HandleGridChanged()
    {
        RecalculateCellsAndAnimate();
        RefreshTopLine();
        if (_canvasSwiper)
        {
            _canvasSwiper.SwitchToCanvas2();
        }
    }

    private void UpdateContentSize()
    {
        if (!contentContainer) return;
        var lineCount = _gridModel.Cells.Count;
        float newHeight = lineCount * _cellSize + Indent;
        contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, newHeight);
    }

    private async void RecalculateCellsAndAnimate()
    {
        try
        {
            if (_isAnimating) return;
            _isAnimating = true;
            UpdateContentSize();
            await RecalculateCellsAsync(MoveDuration);
            _isAnimating = false;
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка пересчета позиций." + e);
        }
    }

    private async Task RecalculateCellsAsync(float duration)
    {
        var animationTasks = new List<Task>();
        for (var i = 0; i < _gridModel.Cells.Count; i++)
        {
            for (var j = 0; j < _gridModel.Cells[i].Count; j++)
            {
                var currentCell = _gridModel.Cells[i][j];
                if (!currentCell) continue;
                currentCell.line = i;
                currentCell.column = j;
                var targetPosition = new Vector2(_cellSize * j + Indent / 2, -_cellSize * i - Indent / 2 - _cellSize * 1.1f);
                var moveTask = AnimateMoveAsync(currentCell.targetRectTransform, targetPosition, duration);
                animationTasks.Add(moveTask);
            }
        }

        await Task.WhenAll(animationTasks);
    }

    private async Task AnimateMoveAsync(RectTransform rectTransform, Vector2 targetPosition, float duration)
    {
        if (!rectTransform) return;
        var startPosition = rectTransform.anchoredPosition;
        var elapsedTime = 0f;
        if (Vector2.Distance(startPosition, targetPosition) < 0.01f) return;
        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        rectTransform.anchoredPosition = targetPosition;
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