using System.Collections.Generic;
using UnityEngine;

public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject canvas;
    private const int QuantityByWidth = 10;
    private const int InitialQuantityByHeight = 5;
    private readonly List<List<Cell>> _cells = new();
    private int _screenWidth;
    private int _cellSize;
    private const int Indent = 10;
    private RectTransform _targetRectTransform;

    private void OnEnable()
    {
        ActionBus.OnCheckLines += CheckLines;
    }

    private void OnDisable()
    {
        ActionBus.OnCheckLines -= CheckLines;
    }

    private void Start()
    {
        _screenWidth = Screen.width - Indent;
        _cellSize = _screenWidth / QuantityByWidth;

        _targetRectTransform = cellPrefab.GetComponent<RectTransform>();
        _targetRectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);

        CreateInitialField(InitialQuantityByHeight);
    }

    private void CreateInitialField(int numberLines)
    {
        for (var i = 0; i < numberLines; i++)
        {
            CreateLine();
        }

        SetLineAndColumn();
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
        // var number = Random.Range(1, 10);
        var number = 1;

        var cell = cellOj.GetComponent<Cell>();


        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.targetRectTransform = cellOj.GetComponent<RectTransform>();

        return cell;
    }

    private void SetLineAndColumn()
    {
        for (var i = 0; i < _cells.Count; i++)
        {
            for (var j = 0; j < _cells[i].Count; j++)
            {
                _cells[i][j].line = i;
                _cells[i][j].column = j;
                _cells[i][j].targetRectTransform.anchoredPosition = new Vector2(_cellSize * j + Indent / 2, -_cellSize * i - Indent / 2);
            }
        }
    }

    private void CheckLines(int line1, int line2)
    {
        var remove1 = true;
        for (var i = 0; i < _cells[line1].Count; i++)
        {
            if (_cells[line1][i].gameObject.activeSelf)
            {
                remove1 = false;
                break;
            }
        }

        var remove2 = true;
        for (var i = 0; i < _cells[line2].Count; i++)
        {
            if (_cells[line2][i].gameObject.activeSelf)
            {
                remove2 = false;
                break;
            }
        }

        if (remove1)
        {
            RemoveLastLine(line1);
        }

        if (remove2 && line1 != line2)
        {
            RemoveLastLine(line2);
        }

        if (remove1 || remove2)
        {
            SetLineAndColumn();
        }
    }

    /// <summary>
    /// Пример функции для добавления новой строки в конец поля.
    /// </summary>
    public void AddNewLine()
    {
        CreateLine();
    }

    /// <summary>
    /// Пример функции для удаления последней строки.
    /// </summary>
    private void RemoveLastLine(int numberLines)
    {
        if (_cells.Count == 0) return;

        foreach (var cell in _cells[numberLines])
        {
            if (cell && cell.gameObject)
            {
                Destroy(cell.gameObject);
            }
        }

        _cells.RemoveAt(numberLines);
    }
}