using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет пулом объектов для ячеек (Cell), чтобы избежать постоянного создания и удаления объектов.
/// </summary>
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
    /// <returns>Активный объект ячейки.</returns>
    public Cell GetCell()
    {
        if (_pooledCells.Count > 0)
        {
            var cell = _pooledCells.Dequeue();
            cell.transform.SetParent(canvasTransform, false);
            cell.gameObject.SetActive(true);
            return cell;
        }

        var cellOj = Instantiate(cellPrefab, canvasTransform, false);
        return cellOj.GetComponent<Cell>();
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