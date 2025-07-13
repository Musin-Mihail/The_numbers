using System.Collections.Generic;
using Core.Events;
using Model;
using UnityEngine;
using View.Grid;

namespace View.UI
{
    /// <summary>
    /// Управляет отображением верхней строки с числами, дублирующими числа на сетке.
    /// </summary>
    public class HeaderNumberDisplay : MonoBehaviour
    {
        [Header("Scene Dependencies")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform container;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private readonly List<Cell> _topLineCells = new();
        private float _cellSize;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onToggleTopLine.AddListener(SetContainerActive);
            }
        }

        private void Start()
        {
            if (!cellPrefab)
            {
                Debug.LogError("Ошибка: 'cellPrefab' не назначен в инспекторе!", this);
                enabled = false;
                return;
            }

            _cellSize = cellPrefab.GetComponent<RectTransform>().sizeDelta.x;

            CreateLineDisplay();
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onToggleTopLine.RemoveListener(SetContainerActive);
            }
        }

        private void SetContainerActive(bool isActive)
        {
            if (container)
            {
                container.gameObject.SetActive(isActive);
            }
        }

        private void CreateLineDisplay()
        {
            if (!cellPrefab)
            {
                Debug.LogError("Ошибка: 'cellPrefab' не назначен в инспекторе для HeaderNumberDisplay!", this);
                return;
            }

            for (var i = 0; i < GameConstants.QuantityByWidth; i++)
            {
                var cellGo = Instantiate(cellPrefab, container);
                var cell = cellGo.GetComponent<Cell>();

                cell.SetSelected(false);
                cell.SetVisualState(false);
                _topLineCells.Add(cell);
                cellGo.SetActive(true);
                var rectTransform = cell.TargetRectTransform;
                rectTransform.anchoredPosition = new Vector2(_cellSize * i + GameConstants.Indent / 2f, -GameConstants.Indent / 2f);
            }
        }

        /// <summary>
        /// Обновляет отображаемые числа в верхней строке.
        /// </summary>
        /// <param name="numbers">Список чисел для отображения.</param>
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