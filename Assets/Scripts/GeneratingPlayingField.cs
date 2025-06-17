using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject canvas;
    private const int QuantityByWidth = 10;
    private const int InitialQuantityByHeight = 5;
    private const int Indent = 10;
    private const float MoveDuration = 0.3f;
    private readonly List<List<Cell>> _cells = new();
    private int _screenWidth;
    private int _cellSize;
    private RectTransform _targetRectTransform;
    private bool isAnimating;

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
        CreateInitialField(InitialQuantityByHeight);
    }

    /// <summary>
    /// Обработчик события проверки линий.
    /// Был переименован из CheckLines, чтобы избежать путаницы с локальной функцией.
    /// </summary>
    private void OnCheckLines(int line1, int line2)
    {
        CheckAndRemoveEmptyLines(line1, line2);
    }

    private void CreateInitialField(int numberLines)
    {
        for (var i = 0; i < numberLines; i++)
        {
            CreateLine();
        }

        RecalculateCellsAndAnimate();
    }

    private void CreateLine()
    {
        var newLine = new List<Cell>();
        _cells.Add(newLine);
        for (var i = 0; i < QuantityByWidth; i++)
        {
            var newCell = CreateCell();
            newLine.Add(newCell);
        }
    }

    private Cell CreateCell()
    {
        var cellOj = Instantiate(cellPrefab, canvas.transform, false);
        var number = Random.Range(1, 10);
        var cell = cellOj.GetComponent<Cell>();
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.targetRectTransform = cellOj.GetComponent<RectTransform>();
        return cell;
    }

    /// <summary>
    /// Публичный метод-обертка для безопасного запуска асинхронной операции.
    /// </summary>
    private async void RecalculateCellsAndAnimate()
    {
        try
        {
            if (isAnimating)
            {
                return;
            }

            isAnimating = true;
            await RecalculateCellsAsync(MoveDuration);
            isAnimating = false;
        }
        catch (Exception e)
        {
            Debug.Log("Ошибка пересчета позиций." + e);
        }
    }

    /// <summary>
    /// Асинхронно пересчитывает и плавно анимирует позиции всех ячеек.
    /// </summary>
    private async Task RecalculateCellsAsync(float duration)
    {
        var animationTasks = new List<Task>();
        for (var i = 0; i < _cells.Count; i++)
        {
            for (var j = 0; j < _cells[i].Count; j++)
            {
                var currentCell = _cells[i][j];
                if (!currentCell || !currentCell.gameObject.activeSelf) continue;
                currentCell.line = i;
                currentCell.column = j;
                var targetPosition = new Vector2(_cellSize * j + Indent / 2, -_cellSize * i - Indent / 2);
                var moveTask = AnimateMoveAsync(currentCell.targetRectTransform, targetPosition, duration);
                animationTasks.Add(moveTask);
            }
        }

        await Task.WhenAll(animationTasks);
    }

    /// <summary>
    /// Асинхронно анимирует перемещение одного RectTransform к целевой позиции.
    /// </summary>
    private async Task AnimateMoveAsync(RectTransform rectTransform, Vector2 targetPosition, float duration)
    {
        if (!rectTransform) return;
        var startPosition = rectTransform.anchoredPosition;
        var elapsedTime = 0f;
        if (Vector2.Distance(startPosition, targetPosition) < 0.01f)
        {
            return;
        }

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
            linesToRemove.Sort((a, b) => b.CompareTo(a)); // Сортировка по убыванию
            foreach (var lineIndex in linesToRemove)
            {
                RemoveLine(lineIndex);
            }

            RecalculateCellsAndAnimate(); // Запускаем пересчет позиций с анимацией
        }
    }

    private bool IsLineEmpty(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= _cells.Count) return false;

        for (var i = 0; i < _cells[lineIndex].Count; i++)
        {
            if (_cells[lineIndex][i] && _cells[lineIndex][i].gameObject.activeSelf)
            {
                return false;
            }
        }

        return true;
    }

    public void AddNewLine()
    {
        CreateLine();
        RecalculateCellsAndAnimate(); // Анимируем при добавлении новой линии
    }

    private void RemoveLine(int numberLine)
    {
        if (numberLine < 0 || numberLine >= _cells.Count) return;

        foreach (var cell in _cells[numberLine])
        {
            if (cell && cell.gameObject)
            {
                Destroy(cell.gameObject);
            }
        }

        _cells.RemoveAt(numberLine);
    }
}