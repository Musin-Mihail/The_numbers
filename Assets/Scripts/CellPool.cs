using System.Collections.Generic;
using UnityEngine;

public class CellPool : MonoBehaviour
{
    public static CellPool Instance { get; private set; }
    [HideInInspector] public GameObject cellPrefab;
    [HideInInspector] public Transform canvasTransform;
    private readonly Queue<Cell> _pooledCells = new();

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Получает ячейку из пула. Если в пуле нет доступных ячеек, создает новую.
    /// </summary>
    /// <returns>Активный объект ячейки с восстановленным видом.</returns>
    public Cell GetCell()
    {
        Cell cell;
        if (_pooledCells.Count > 0)
        {
            cell = _pooledCells.Dequeue();
            cell.transform.SetParent(canvasTransform, false);
        }
        else
        {
            var cellOj = Instantiate(cellPrefab, canvasTransform, false);
            cell = cellOj.GetComponent<Cell>();
        }

        cell.EnableCell();
        return cell;
    }

    /// <summary>
    /// Возвращает ячейку в пул для повторного использования.
    /// </summary>
    /// <param name="cell">Ячейка для возврата в пул.</param>
    public void ReturnCell(Cell cell)
    {
        if (!cell) return;
        cell.gameObject.SetActive(false);
        _pooledCells.Enqueue(cell);
    }
}