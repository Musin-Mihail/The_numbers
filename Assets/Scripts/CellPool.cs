using System.Collections.Generic;
using UnityEngine;

public class CellPool : MonoBehaviour
{
    private static CellPool Instance { get; set; }
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

        cell.gameObject.SetActive(true);
        return cell;
    }

    public void ReturnCell(Cell cell)
    {
        if (!cell) return;
        cell.gameObject.SetActive(false);
        _pooledCells.Enqueue(cell);
    }
}