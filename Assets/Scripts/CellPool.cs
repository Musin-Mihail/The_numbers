using System.Collections.Generic;
using UnityEngine;

public class CellPool : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform canvasTransform;
    private readonly Queue<Cell> _pooledCells = new();

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
            cell = cellObj.GetComponent<Cell>();
        }

        cell.transform.SetParent(canvasTransform, false);
        cell.gameObject.SetActive(true);
        return cell;
    }

    public void ReturnCell(Cell cell)
    {
        if (!cell) return;
        cell.gameObject.SetActive(false);
        cell.ResetForPooling();
        _pooledCells.Enqueue(cell);
    }
}