using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopLineController : MonoBehaviour
{
    public GameObject cellPrefab;
    public RectTransform container;

    private readonly List<Cell> _topLineCells = new();
    private const int QuantityByWidth = 10;
    private const int Indent = 10;
    private int _cellSize;

    private void Start()
    {
        if (!container)
        {
            container = GetComponent<RectTransform>();
        }

        _cellSize = (Screen.width - Indent) / QuantityByWidth;
        CreateLineDisplay();
    }

    private void CreateLineDisplay()
    {
        for (int i = 0; i < QuantityByWidth; i++)
        {
            var cellGo = Instantiate(cellPrefab, container);
            var cell = cellGo.GetComponent<Cell>();
            cell.targetRectTransform = cellGo.GetComponent<RectTransform>();
            if (cell.TryGetComponent<Button>(out var button))
            {
                button.enabled = false;
            }

            cell.indicator.SetActive(false);
            _topLineCells.Add(cell);
            cellGo.SetActive(true);
        }
    }

    public void UpdateDisplayedNumbers(List<int> numbers)
    {
        for (int i = 0; i < _topLineCells.Count; i++)
        {
            var cell = _topLineCells[i];
            cell.text.text = numbers[i].ToString();
            cell.targetRectTransform.anchoredPosition = new Vector2(_cellSize * i + Indent / 2, 0 - Indent / 2);

            if (numbers[i] == 0)
            {
                cell.SetVisualState(false);
            }
            else
            {
                cell.SetVisualState(true);
            }
        }
    }
}