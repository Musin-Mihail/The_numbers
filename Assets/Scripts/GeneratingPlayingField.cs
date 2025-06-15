using UnityEngine;

public class GeneratingPlayingField : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject canvas;
    private const int QuantityByWidth = 10;
    private const int QuantityByHeight = 5;
    private int _screenWidth;
    private int _cellSize;
    private const int Indent = 10;
    private int _numberLines;
    private RectTransform _targetRectTransform;

    private void Start()
    {
        _screenWidth = Screen.width - Indent;
        _cellSize = _screenWidth / QuantityByWidth;

        _targetRectTransform = cellPrefab.GetComponent<RectTransform>();
        _targetRectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);
        CreateLines(QuantityByHeight);
    }

    private void CreateLines(int numberLines)
    {
        for (var i = 0; i < numberLines; i++)
        {
            CreateLine();
        }
    }

    private void CreateLine()
    {
        for (var i = 0; i < QuantityByWidth; i++)
        {
            _targetRectTransform.anchoredPosition = new Vector2(_cellSize * i + Indent / 2, -_cellSize * _numberLines - Indent / 2);
            CreateCell(i);
        }

        _numberLines++;
    }

    private void CreateCell(int column)
    {
        var cellOj = Instantiate(cellPrefab, canvas.transform, false);
        var number = Random.Range(1, 10);
        var cell = cellOj.GetComponent<Cell>();
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.line = _numberLines;
        cell.column = column;
    }
}