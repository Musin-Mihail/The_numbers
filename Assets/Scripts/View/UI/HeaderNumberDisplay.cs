using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.Grid;

namespace View.UI
{
    public class HeaderNumberDisplay : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform container;

        private readonly List<Cell> _topLineCells = new();
        private float _cellSize;

        private void Start()
        {
            if (!canvasScaler)
            {
                Debug.LogError("Ошибка: CanvasScaler не назначен в инспекторе!", this);
                enabled = false;
                return;
            }

            var referenceWidth = canvasScaler.referenceResolution.x;
            _cellSize = (referenceWidth - GameConstants.Indent) / GameConstants.QuantityByWidth;

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
                var rectTransform = cell.targetRectTransform;
                rectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);
                rectTransform.anchoredPosition = new Vector2(_cellSize * i + GameConstants.Indent / 2f, -GameConstants.Indent / 2f);
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
}