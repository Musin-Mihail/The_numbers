using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridModel
{
    public List<List<Cell>> Cells { get; } = new();
    private const int QuantityByWidth = 10;
    private readonly CellPool _cellPool;

    /// <summary>
    /// Конструктор модели, который принимает пул ячеек для создания новых экземпляров.
    /// </summary>
    public GridModel(CellPool cellPool)
    {
        _cellPool = cellPool;
    }

    /// <summary>
    /// Возвращает плоский список всех активных ячеек.
    /// </summary>
    public List<Cell> GetAllActiveCells()
    {
        return Cells
            .SelectMany(line => line)
            .Where(cell => cell && cell.IsActive)
            .ToList();
    }

    /// <summary>
    /// Проверяет, находятся ли ячейки на одной линии или в одном столбце без активных ячеек между ними.
    /// </summary>
    public bool AreCellsOnSameLineOrColumnWithoutGaps(Cell firstCell, Cell secondCell)
    {
        var onSameLine = firstCell.line == secondCell.line;
        var onSameColumn = firstCell.column == secondCell.column;

        if (!onSameLine && !onSameColumn)
        {
            return false;
        }

        if (onSameLine)
        {
            var line = firstCell.line;
            var startCol = Mathf.Min(firstCell.column, secondCell.column);
            var endCol = Mathf.Max(firstCell.column, secondCell.column);
            for (var c = startCol + 1; c < endCol; c++)
            {
                if (c < Cells[line].Count && Cells[line][c].IsActive)
                {
                    return false;
                }
            }
        }
        else // onSameColumn
        {
            var col = firstCell.column;
            var startLine = Mathf.Min(firstCell.line, secondCell.line);
            var endLine = Mathf.Max(firstCell.line, secondCell.line);
            for (var l = startLine + 1; l < endLine; l++)
            {
                if (l < Cells.Count && col < Cells[l].Count && Cells[l][col].IsActive)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Очищает поле, возвращая все ячейки в пул.
    /// </summary>
    public void ClearField()
    {
        foreach (var cell in Cells.SelectMany(line => line).Where(cell => cell))
        {
            _cellPool.ReturnCell(cell);
        }

        Cells.Clear();
    }

    /// <summary>
    /// Создает новую линию с ячейками.
    /// </summary>
    public void CreateLine()
    {
        var newLine = new List<Cell>();
        Cells.Add(newLine);
        for (var i = 0; i < QuantityByWidth; i++)
        {
            var newCell = CreateCell();
            newLine.Add(newCell);
        }
    }

    /// <summary>
    /// Создает ячейку со случайным числом.
    /// </summary>
    private Cell CreateCell()
    {
        var cell = _cellPool.GetCell();
        var number = Random.Range(1, 10);
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.OnDeselectingCell();
        return cell;
    }

    /// <summary>
    /// Создает ячейку с заданным числом.
    /// </summary>
    public Cell CreateCellWithNumber(int number)
    {
        var cell = _cellPool.GetCell();
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.OnDeselectingCell();
        return cell;
    }

    /// <summary>
    /// Проверяет, является ли линия полностью неактивной.
    /// </summary>
    public bool IsLineEmpty(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Cells.Count) return false;
        return Cells[lineIndex].All(cell => !cell || !cell.IsActive);
    }

    /// <summary>
    /// Удаляет линию и возвращает ее ячейки в пул.
    /// </summary>
    public void RemoveLine(int numberLine)
    {
        if (numberLine < 0 || numberLine >= Cells.Count) return;
        foreach (var cell in Cells[numberLine].Where(cell => cell))
        {
            _cellPool.ReturnCell(cell);
        }

        Cells.RemoveAt(numberLine);
    }
}