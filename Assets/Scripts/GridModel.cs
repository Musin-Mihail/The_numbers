using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridModel : IGridDataProvider
{
    public event Action<CellData> OnCellAdded;
    public event Action<CellData> OnCellUpdated;
    public event Action<Guid> OnCellRemoved;
    public event Action OnGridCleared;

    public List<List<CellData>> Cells { get; } = new();
    private readonly Dictionary<Guid, CellData> _cellDataMap = new();

    public CellData GetCellDataById(Guid id)
    {
        _cellDataMap.TryGetValue(id, out var data);
        return data;
    }

    public void SetCellActiveState(CellData data, bool isActive)
    {
        if (data.IsActive == isActive) return;
        data.SetActive(isActive);
        OnCellUpdated?.Invoke(data);
    }

    public void ClearField()
    {
        Cells.Clear();
        _cellDataMap.Clear();
        OnGridCleared?.Invoke();
    }

    public void CreateLine(int lineIndex)
    {
        var newLine = new List<CellData>();
        for (var i = 0; i < GameConstants.QuantityByWidth; i++)
        {
            var cellData = new CellData(Random.Range(1, 10), lineIndex, i);
            newLine.Add(cellData);
            _cellDataMap[cellData.Id] = cellData;
            OnCellAdded?.Invoke(cellData);
        }

        Cells.Add(newLine);
    }

    public bool IsLineEmpty(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Cells.Count) return false;
        return Cells[lineIndex].All(cellData => !cellData.IsActive);
    }

    public void RemoveLine(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Cells.Count) return;
        foreach (var cellData in Cells[lineIndex])
        {
            _cellDataMap.Remove(cellData.Id);
            OnCellRemoved?.Invoke(cellData.Id);
        }

        Cells.RemoveAt(lineIndex);
        for (var i = lineIndex; i < Cells.Count; i++)
        {
            foreach (var cell in Cells[i])
            {
                cell.Line = i;
                OnCellUpdated?.Invoke(cell);
            }
        }
    }

    public List<int> GetNumbersForTopLine(int numberLine)
    {
        var topNumbers = new List<int>(new int[GameConstants.QuantityByWidth]);
        if (numberLine < 0) return topNumbers;

        var activeCells = GetAllActiveCellData(); // Используем свой же метод

        numberLine = Mathf.Min(numberLine, Cells.Count);
        for (var col = 0; col < GameConstants.QuantityByWidth; col++)
        {
            for (var line = numberLine - 1; line >= 0; line--)
            {
                var cellData = activeCells.LastOrDefault(d => d.Column == col && d.Line == line);
                if (cellData != null)
                {
                    topNumbers[col] = cellData.Number;
                    break;
                }
            }
        }

        return topNumbers;
    }

    public void AppendActiveNumbersToGrid()
    {
        var numbersToAdd = GetAllActiveCellData().Select(cell => cell.Number).ToList();
        if (numbersToAdd.Count == 0) return;
        var lastLine = Cells.LastOrDefault();
        if (lastLine != null)
        {
            for (var i = lastLine.Count - 1; i >= 0; i--)
            {
                var cell = lastLine[i];
                if (!cell.IsActive)
                {
                    _cellDataMap.Remove(cell.Id);
                    OnCellRemoved?.Invoke(cell.Id);
                    lastLine.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
        }

        var lineIndex = Cells.Count > 0 ? Cells.Count - 1 : 0;
        var columnIndex = Cells.Count > 0 && Cells[lineIndex] != null ? Cells[lineIndex].Count : 0;
        if (Cells.Count == 0)
        {
            Cells.Add(new List<CellData>());
        }

        foreach (var number in numbersToAdd)
        {
            if (columnIndex >= GameConstants.QuantityByWidth)
            {
                columnIndex = 0;
                lineIndex++;
                if (Cells.Count <= lineIndex)
                {
                    Cells.Add(new List<CellData>());
                }
            }

            var newCellData = new CellData(number, lineIndex, columnIndex);
            Cells[lineIndex].Add(newCellData);
            _cellDataMap[newCellData.Id] = newCellData;
            OnCellAdded?.Invoke(newCellData);
            columnIndex++;
        }
    }

    public List<CellData> GetAllActiveCellData()
    {
        return _cellDataMap.Values.Where(cell => cell.IsActive).ToList();
    }

    public bool AreCellsOnSameLineOrColumnWithoutGaps(CellData firstCell, CellData secondCell)
    {
        var onSameLine = firstCell.Line == secondCell.Line;
        var onSameColumn = firstCell.Column == secondCell.Column;

        if (!onSameLine && !onSameColumn) return false;

        if (onSameLine)
        {
            var line = firstCell.Line;
            var startCol = Mathf.Min(firstCell.Column, secondCell.Column);
            var endCol = Mathf.Max(firstCell.Column, secondCell.Column);
            return !_cellDataMap.Values.Any(cell => cell.IsActive && cell.Line == line && cell.Column > startCol && cell.Column < endCol);
        }

        var col = firstCell.Column;
        var startLine = Mathf.Min(firstCell.Line, secondCell.Line);
        var endLine = Mathf.Max(firstCell.Line, secondCell.Line);

        return !_cellDataMap.Values.Any(cell => cell.IsActive && cell.Column == col && cell.Line > startLine && cell.Line < endLine);
    }
}