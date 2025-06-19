using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CellPool))]
public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public RectTransform contentContainer;
    public CanvasSwiper canvasSwiper;
    public ScrollRect scrollRect;
    public TopLineController topLineController;
    public float scrollLoggingThreshold = 20f;

    private GridModel _gridModel;
    private GameController _gameController;
    private CalculatingMatches _calculatingMatches;
    private CellPool _cellPool;

    private const int QuantityByWidth = 10;
    private const int InitialQuantityByHeight = 5;
    private const int Indent = 10;
    private const float MoveDuration = 0.3f;
    private int _screenWidth;
    private int _cellSize;
    private bool _isAnimating;
    private float _lastLoggedScrollPosition;

    private Cell _firstSelectedCell;
    private Cell _secondSelectedCell;

    private void Awake()
    {
        _cellPool = GetComponent<CellPool>();
        if (_cellPool)
        {
            _cellPool.cellPrefab = cellPrefab;
            _cellPool.canvasTransform = contentContainer;
        }

        _gridModel = new GridModel(_cellPool, OnCellCreated);
        _calculatingMatches = new CalculatingMatches(_gridModel);
        _gameController = new GameController(_gridModel, _calculatingMatches);
    }

    private void OnEnable()
    {
        if (scrollRect)
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        _gameController.OnMatchFound += HandleMatchFound;
        _gameController.OnInvalidMatch += HandleInvalidMatch;
        _gameController.OnGridChanged += HandleGridChanged;
    }

    private void OnDisable()
    {
        if (scrollRect)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        _gameController.OnMatchFound -= HandleMatchFound;
        _gameController.OnInvalidMatch -= HandleInvalidMatch;
        _gameController.OnGridChanged -= HandleGridChanged;
    }

    private void Start()
    {
        _screenWidth = Screen.width - Indent;
        _cellSize = _screenWidth / QuantityByWidth;
        scrollLoggingThreshold = _cellSize / 2;

        if (scrollRect)
        {
            _lastLoggedScrollPosition = scrollRect.content.anchoredPosition.y;
        }
    }

    private void OnCellCreated(Cell cell)
    {
        cell.OnCellClicked += OnCellClicked;
    }

    private void OnCellClicked(Cell cell)
    {
        if (_isAnimating) return;
        _gameController.HandleCellSelection(cell);
    }

    private void HandleMatchFound(Cell firstCell, Cell secondCell)
    {
        RefreshTopLine();
    }

    private void HandleInvalidMatch()
    {
        foreach (var cell in _gridModel.GetAllActiveCells())
        {
            cell.SetSelected(false);
        }
    }

    private void HandleGridChanged()
    {
        RecalculateCellsAndAnimate();
        RefreshTopLine();
    }

    public void StartNewGame()
    {
        if (_isAnimating)
        {
            _isAnimating = false;
            StopAllCoroutines();
        }

        _gameController.ResetGame();
        foreach (var cell in _gridModel.GetAllActiveCells())
        {
            if (cell)
            {
                cell.OnCellClicked -= OnCellClicked;
            }
        }

        _gridModel.ClearField();
        for (var i = 0; i < InitialQuantityByHeight; i++)
        {
            _gridModel.CreateLine();
        }

        UpdateContentSize();
        RecalculateCellsAndAnimate();
        RefreshTopLine();
        if (canvasSwiper)
        {
            canvasSwiper.SwitchToCanvas2();
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
        if (!topLineController) return;
        var numberLine = (int)Math.Floor(_lastLoggedScrollPosition / _cellSize);
        var activeNumbers = new List<int>();
        if (numberLine <= 0)
        {
            activeNumbers.AddRange(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            topLineController.UpdateDisplayedNumbers(activeNumbers);
            return;
        }

        if (numberLine > _gridModel.Cells.Count)
        {
            numberLine = _gridModel.Cells.Count;
        }

        if (numberLine == 0)
        {
            for (var i = 0; i < QuantityByWidth; i++)
            {
                if (_gridModel.Cells[numberLine][i].IsActive)
                {
                    activeNumbers.Add(_gridModel.Cells[numberLine][i].number);
                }
                else
                {
                    activeNumbers.Add(0);
                }
            }
        }
        else
        {
            for (var i = 0; i < QuantityByWidth; i++)
            {
                for (var l = numberLine - 1; l >= 0; l--)
                {
                    if (l < _gridModel.Cells.Count && i < _gridModel.Cells[l].Count && _gridModel.Cells[l][i].IsActive)
                    {
                        activeNumbers.Add(_gridModel.Cells[l][i].number);
                        break;
                    }

                    if (l == 0)
                    {
                        activeNumbers.Add(0);
                    }
                }
            }
        }

        topLineController.UpdateDisplayedNumbers(activeNumbers);
    }

    private void OnScrollValueChanged(Vector2 position)
    {
        var currentScrollPosition = scrollRect.content.anchoredPosition.y;

        if (Mathf.Abs(currentScrollPosition - _lastLoggedScrollPosition) >= scrollLoggingThreshold)
        {
            _lastLoggedScrollPosition = currentScrollPosition;
            RefreshTopLine();
        }
    }

    public void AddExistingNumbersAsNewLines()
    {
        var numbersToAdd = _gridModel.GetAllActiveCells()
            .Select(cell => cell.number)
            .ToList();

        if (numbersToAdd.Count == 0) return;

        _gridModel.AddNumbersAsNewLines(numbersToAdd);

        RecalculateCellsAndAnimate();
        RefreshTopLine();
        if (canvasSwiper)
        {
            canvasSwiper.SwitchToCanvas2();
        }
    }
}