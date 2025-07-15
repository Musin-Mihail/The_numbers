using UnityEngine;
using UnityEngine.UI;
using View.Grid;

namespace View.UI
{
    /// <summary>
    /// Генерирует и отображает демонстрационную сетку в окне правил,
    /// подсвечивая примеры допустимых пар.
    /// </summary>
    public class RulesGrid : MonoBehaviour
    {
        [Header("Настройки сетки")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform gridContainer;
        private const int GridSize = 5;

        [Header("Цвета подсветки")]
        [SerializeField] private Color sameNumberColor = Color.yellow;
        [SerializeField] private Color sumIsTenColor = Color.cyan;
        [SerializeField] private Color lineWrapColor = Color.magenta;
        [SerializeField] private Color firstAndLastColor = Color.green;

        private Cell[,] _cells;
        private int[,] _gridNumbers;

        /// <summary>
        /// Генерирует демонстрационную сетку с предопределенными числами.
        /// </summary>
        public void GenerateGrid()
        {
            ClearGrid();
            _cells = new Cell[GridSize, GridSize];
            _gridNumbers = new int[GridSize, GridSize];
            _gridNumbers[2, 1] = 7;
            _gridNumbers[2, 2] = 7;
            _gridNumbers[0, 3] = 3;
            _gridNumbers[1, 3] = 7;
            _gridNumbers[2, 4] = 5;
            _gridNumbers[3, 0] = 5;
            _gridNumbers[0, 0] = 1;
            _gridNumbers[4, 4] = 1;
            FillRemainingCells();
            InstantiateGrid();
            HighlightPairs();
        }

        private void FillRemainingCells()
        {
            int[] fillerNumbers = { 2, 4, 6, 8, 9 };
            var fillerIndex = 0;
            for (var y = 0; y < GridSize; y++)
            {
                for (var x = 0; x < GridSize; x++)
                {
                    if (_gridNumbers[y, x] != 0) continue;
                    _gridNumbers[y, x] = fillerNumbers[fillerIndex % fillerNumbers.Length];
                    fillerIndex++;
                }
            }
        }

        private void InstantiateGrid()
        {
            if (!cellPrefab || !gridContainer)
            {
                Debug.LogError("Префаб ячейки или контейнер сетки не назначены.");
                return;
            }

            var cellSize = cellPrefab.GetComponent<RectTransform>().sizeDelta.x;
            const float spacing = 5f;

            for (var y = 0; y < GridSize; y++)
            {
                for (var x = 0; x < GridSize; x++)
                {
                    var cellGo = Instantiate(cellPrefab, gridContainer);
                    var cell = cellGo.GetComponent<Cell>();
                    _cells[y, x] = cell;

                    cell.text.text = _gridNumbers[y, x].ToString();
                    cell.SetVisualState(true);

                    var rectTransform = cell.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(x * (cellSize + spacing), -y * (cellSize + spacing));
                }
            }
        }

        private void HighlightPairs()
        {
            HighlightCell(2, 1, sameNumberColor);
            HighlightCell(2, 2, sameNumberColor);
            HighlightCell(0, 3, sumIsTenColor);
            HighlightCell(1, 3, sumIsTenColor);
            HighlightCell(2, 4, lineWrapColor);
            HighlightCell(3, 0, lineWrapColor);
            HighlightCell(0, 0, firstAndLastColor);
            HighlightCell(4, 4, firstAndLastColor);
        }

        private void HighlightCell(int y, int x, Color color)
        {
            if (y < GridSize && x < GridSize && _cells[y, x])
            {
                _cells[y, x].GetComponent<Image>().color = color;
            }
        }

        private void ClearGrid()
        {
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            _cells = null;
        }
    }
}