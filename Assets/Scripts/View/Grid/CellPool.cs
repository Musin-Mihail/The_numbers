using System.Collections.Generic;
using UnityEngine;

namespace View.Grid
{
    /// <summary>
    /// Реализует пул объектов для ячеек (Cell) для оптимизации производительности.
    /// </summary>
    public class CellPool : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Transform canvasTransform;
        private readonly Queue<Cell> _pooledCells = new();

        /// <summary>
        /// Получает ячейку из пула или создает новую, если пул пуст.
        /// </summary>
        /// <returns>Экземпляр ячейки.</returns>
        public Cell GetCell()
        {
            Cell cell;
            if (_pooledCells.Count > 0)
            {
                cell = _pooledCells.Dequeue();
            }
            else
            {
                var cellObj = Instantiate(cellPrefab, canvasTransform);
                cellObj.transform.SetAsFirstSibling();
                cell = cellObj.GetComponent<Cell>();
            }

            cell.transform.SetParent(canvasTransform, false);
            cell.gameObject.SetActive(true);
            return cell;
        }

        /// <summary>
        /// Возвращает ячейку в пул для повторного использования.
        /// </summary>
        /// <param name="cell">Ячейка для возврата.</param>
        public void ReturnCell(Cell cell)
        {
            if (!cell) return;
            cell.gameObject.SetActive(false);
            cell.ResetForPooling();
            _pooledCells.Enqueue(cell);
        }
    }
}