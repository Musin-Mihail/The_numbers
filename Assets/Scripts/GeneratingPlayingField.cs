using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CellPool))]
public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject canvas;
    public CanvasSwiper canvasSwiper;
    public List<List<Cell>> Cells { get; } = new();
    public static GeneratingPlayingField Instance { get; private set; }
    private const int QuantityByWidth = 10;
    private const int InitialQuantityByHeight = 5;
    private const int Indent = 10;
    private const float MoveDuration = 0.3f;
    private int _screenWidth;
    private int _cellSize;
    private RectTransform _targetRectTransform;
    private bool _isAnimating;
    private CellPool _cellPool;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            _cellPool = GetComponent<CellPool>();
            if (_cellPool)
            {
                _cellPool.cellPrefab = cellPrefab;
                _cellPool.canvasTransform = canvas.transform;
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        ActionBus.OnCheckLines += OnCheckLines;
    }

    private void OnDisable()
    {
        ActionBus.OnCheckLines -= OnCheckLines;
    }

    private void Start()
    {
        _screenWidth = Screen.width - Indent;
        _cellSize = _screenWidth / QuantityByWidth;
        _targetRectTransform = cellPrefab.GetComponent<RectTransform>();
        _targetRectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);
    }

    public void StartNewGame()
    {
        if (_isAnimating)
        {
            _isAnimating = false;
            StopAllCoroutines();
        }

        ClearField();
        for (var i = 0; i < InitialQuantityByHeight; i++)
        {
            CreateLine();
        }

        RecalculateCellsAndAnimate();
        if (canvasSwiper)
        {
            canvasSwiper.SwitchToCanvas2();
        }
        else
        {
            Debug.LogWarning("CanvasSwiper не назначен в инспекторе для GeneratingPlayingField!");
        }
    }

    private void ClearField()
    {
        foreach (var cell in Cells.SelectMany(line => line).Where(cell => cell))
        {
            _cellPool.ReturnCell(cell);
        }

        Cells.Clear();
    }

    private void OnCheckLines(int line1, int line2)
    {
        CheckAndRemoveEmptyLines(line1, line2);
    }

    private void CreateLine()
    {
        var newLine = new List<Cell>();
        Cells.Add(newLine);
        for (var i = 0; i < QuantityByWidth; i++)
        {
            var newCell = CreateCell();
            newLine.Add(newCell);
        }
    }

    private Cell CreateCell()
    {
        var cell = _cellPool.GetCell();
        var number = Random.Range(1, 10);
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.OnDeselectingCell();
        return cell;
    }

    private Cell CreateCellWithNumber(int number)
    {
        var cell = _cellPool.GetCell();
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.OnDeselectingCell();
        return cell;
    }

    private async void RecalculateCellsAndAnimate()
    {
        try
        {
            if (_isAnimating) return;
            _isAnimating = true;
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
        for (var i = 0; i < Cells.Count; i++)
        {
            for (var j = 0; j < Cells[i].Count; j++)
            {
                var currentCell = Cells[i][j];
                if (!currentCell || !currentCell.IsActive) continue;
                currentCell.line = i;
                currentCell.column = j;
                var targetPosition = new Vector2(_cellSize * j + Indent / 2, -_cellSize * i - Indent / 2);
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

    private void CheckAndRemoveEmptyLines(int line1, int line2)
    {
        var line1Empty = IsLineEmpty(line1);
        var line2Empty = IsLineEmpty(line2);
        var linesToRemove = new List<int>();
        if (line1Empty) linesToRemove.Add(line1);
        if (line2Empty && line1 != line2) linesToRemove.Add(line2);

        if (linesToRemove.Count > 0)
        {
            linesToRemove.Sort((a, b) => b.CompareTo(a));
            foreach (var lineIndex in linesToRemove)
            {
                RemoveLine(lineIndex);
            }

            RecalculateCellsAndAnimate();
        }
    }

    private bool IsLineEmpty(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Cells.Count) return false;
        return Cells[lineIndex].All(cell => !cell || !cell.IsActive);
    }

    private void RemoveLine(int numberLine)
    {
        if (numberLine < 0 || numberLine >= Cells.Count) return;
        foreach (var cell in Cells[numberLine].Where(cell => cell))
        {
            _cellPool.ReturnCell(cell);
        }

        Cells.RemoveAt(numberLine);
    }

    public void AddExistingNumbersAsNewLines()
    {
        var numbersToAdd = Cells
            .SelectMany(line => line)
            .Where(cell => cell && cell.IsActive)
            .Select(cell => cell.number)
            .ToList();

        if (numbersToAdd.Count == 0) return;
        var currentLine = Cells.LastOrDefault();
        if (currentLine == null) return;

        for (var i = currentLine.Count - 1; i >= 0; i--)
        {
            var cell = currentLine[i];
            if (cell && cell.IsActive) break;
            if (cell)
            {
                _cellPool.ReturnCell(cell);
            }

            currentLine.RemoveAt(i);
        }

        foreach (var number in numbersToAdd)
        {
            if (currentLine.Count >= QuantityByWidth)
            {
                currentLine = new List<Cell>();
                Cells.Add(currentLine);
            }

            var newCell = CreateCellWithNumber(number);
            currentLine.Add(newCell);
        }

        RecalculateCellsAndAnimate();
        if (canvasSwiper)
        {
            canvasSwiper.SwitchToCanvas2();
        }
        else
        {
            Debug.LogWarning("CanvasSwiper не назначен в инспекторе для GeneratingPlayingField!");
        }
    }
}