using System.Collections.Generic;
using UnityEngine;

public class TopLineController : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform container;

    private readonly List<Cell> _topLineCells = new();
    private int _cellSize;

    private void Start()
    {
        _cellSize = (Screen.width - GameConstants.Indent) / GameConstants.QuantityByWidth;
        CreateLineDisplay();
    }

    private void CreateLineDisplay()
    {
        for (var i = 0; i < GameConstants.QuantityByWidth; i++)
        {
            var cellGo = Instantiate(cellPrefab, container);
            var cell = cellGo.GetComponent<Cell>();
            cell.enabled = false;
            cell.SetSelected(false);
            cell.SetVisualState(false);
            _topLineCells.Add(cell);
            cellGo.SetActive(true);
            cell.targetRectTransform.anchoredPosition = new Vector2(_cellSize * i + GameConstants.Indent / 2f, -GameConstants.Indent / 2f);
        }
    }

    public void UpdateDisplayedNumbers(List<int> numbers)
    {
        for (var i = 0; i < _topLineCells.Count; i++)
        {
            if (i >= numbers.Count) continue;
            var cell = _topLineCells[i];
            var number = numbers[i];
            if (cell.Number != number)
            {
                cell.text.text = number.ToString();
            }

            var isActive = number != 0;
            cell.SetVisualState(isActive);
        }
    }
}