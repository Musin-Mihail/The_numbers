using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridModel : IGridDataProvider
{
    public List<List<Cell>> Cells { get; } = new();
    private const int QuantityByWidth = 10;
    private readonly CellPool _cellPool;
    private readonly Action<Cell> _onCellCreatedCallback;
    private readonly Action<Cell> _onCellCleanupCallback;

    public GridModel(CellPool cellPool, Action<Cell> onCellCreatedCallback = null, Action<Cell> onCellCleanupCallback = null)
    {
        _cellPool = cellPool;
        _onCellCreatedCallback = onCellCreatedCallback;
        _onCellCleanupCallback = onCellCleanupCallback;
    }

    public List<Cell> GetAllActiveCells()
    {
        return Cells
            .SelectMany(line => line)
            .Where(cell => cell && cell.IsActive)
            .ToList();
    }

    public bool AreCellsOnSameLineOrColumnWithoutGaps(Cell firstCell, Cell secondCell)
    {
        var onSameLine = firstCell.line == secondCell.line;
        var onSameColumn = firstCell.column == secondCell.column;

        if (!onSameLine && !onSameColumn) return false;

        if (onSameLine)
        {
            var line = firstCell.line;
            var startCol = Mathf.Min(firstCell.column, secondCell.column);
            var endCol = Mathf.Max(firstCell.column, secondCell.column);
            for (var c = startCol + 1; c < endCol; c++)
            {
                if (c < Cells[line].Count && Cells[line][c].IsActive) return false;
            }
        }
        else // onSameColumn
        {
            var col = firstCell.column;
            var startLine = Mathf.Min(firstCell.line, secondCell.line);
            var endLine = Mathf.Max(firstCell.line, secondCell.line);
            for (var l = startLine + 1; l < endLine; l++)
            {
                if (l < Cells.Count && col < Cells[l].Count && Cells[l][col].IsActive) return false;
            }
        }

        return true;
    }

    public void ClearField()
    {
        foreach (var cell in Cells.SelectMany(line => line).Where(cell => cell))
        {
            _onCellCleanupCallback?.Invoke(cell);
            _cellPool.ReturnCell(cell);
        }

        Cells.Clear();
    }

    public void CreateLine()
    {
        var newLine = new List<Cell>();
        for (var i = 0; i < QuantityByWidth; i++)
        {
            newLine.Add(CreateCell());
        }

        Cells.Add(newLine);
    }

    private Cell CreateCell()
    {
        var cell = _cellPool.GetCell();
        var number = Random.Range(1, 10);
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.OnDeselectingCell();
        _onCellCreatedCallback?.Invoke(cell);
        return cell;
    }

    private Cell CreateCellWithNumber(int number)
    {
        var cell = _cellPool.GetCell();
        cell.number = number;
        cell.text.text = cell.number.ToString();
        cell.OnDeselectingCell();
        _onCellCreatedCallback?.Invoke(cell);
        return cell;
    }

    public bool IsLineEmpty(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Cells.Count) return false;
        return Cells[lineIndex].All(cell => !cell || !cell.IsActive);
    }

    public void RemoveLine(int numberLine)
    {
        if (numberLine < 0 || numberLine >= Cells.Count) return;
        foreach (var cell in Cells[numberLine].Where(cell => cell))
        {
            _onCellCleanupCallback?.Invoke(cell);
            _cellPool.ReturnCell(cell);
        }

        Cells.RemoveAt(numberLine);
    }

    public void AddNumbersAsNewLines(List<int> numbers)
    {
        var currentLine = Cells.LastOrDefault();
        if (currentLine == null)
        {
            currentLine = new List<Cell>();
            Cells.Add(currentLine);
        }

        for (var i = currentLine.Count - 1; i >= 0; i--)
        {
            var cell = currentLine[i];
            if (cell && cell.IsActive) break;
            if (cell)
            {
                _onCellCleanupCallback?.Invoke(cell);
                _cellPool.ReturnCell(cell);
            }

            currentLine.RemoveAt(i);
        }

        foreach (var number in numbers)
        {
            if (currentLine.Count >= QuantityByWidth)
            {
                currentLine = new List<Cell>();
                Cells.Add(currentLine);
            }

            var newCell = CreateCellWithNumber(number);
            currentLine.Add(newCell);
        }
    }

    public List<int> GetNumbersForTopLine(int numberLine, int quantityByWidth)
    {
        var activeNumbers = new List<int>();
        if (numberLine <= 0)
        {
            activeNumbers.AddRange(Enumerable.Repeat(0, quantityByWidth));
            return activeNumbers;
        }

        if (numberLine > Cells.Count)
        {
            numberLine = Cells.Count;
        }

        for (var i = 0; i < quantityByWidth; i++)
        {
            bool found = false;
            for (var l = numberLine - 1; l >= 0; l--)
            {
                if (l < Cells.Count && i < Cells[l].Count && Cells[l][i].IsActive)
                {
                    activeNumbers.Add(Cells[l][i].number);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                activeNumbers.Add(0);
            }
        }

        return activeNumbers;
    }
}