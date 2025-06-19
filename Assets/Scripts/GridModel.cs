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
    private List<CellData> _activeCellsCache = new();
    private bool _isCacheDirty = true;

    public List<CellData> GetAllActiveCellData()
    {
        if (_isCacheDirty)
        {
            _activeCellsCache = _cellDataMap.Values
                .Where(data => data.IsActive)
                .OrderBy(data => data.Line)
                .ThenBy(data => data.Column)
                .ToList();
            _isCacheDirty = false;
        }

        return _activeCellsCache;
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

            var lineData = Cells[line];
            for (var c = startCol + 1; c < endCol; c++)
            {
                var data = lineData.FirstOrDefault(d => d.Column == c);
                if (data is { IsActive: true }) return false;
            }
        }
        else // onSameColumn
        {
            var col = firstCell.Column;
            var startLine = Mathf.Min(firstCell.Line, secondCell.Line);
            var endLine = Mathf.Max(firstCell.Line, secondCell.Line);
            for (var l = startLine + 1; l < endLine; l++)
            {
                if (l < Cells.Count)
                {
                    var data = Cells[l].FirstOrDefault(d => d.Column == col);
                    if (data is { IsActive: true }) return false;
                }
            }
        }

        return true;
    }

    public CellData GetCellDataById(Guid id)
    {
        _cellDataMap.TryGetValue(id, out var data);
        return data;
    }

    public void SetCellActiveState(CellData data, bool isActive)
    {
        data.SetActive(isActive);
        _isCacheDirty = true;
        OnCellUpdated?.Invoke(data);
    }

    public void ClearField()
    {
        Cells.Clear();
        _cellDataMap.Clear();
        _isCacheDirty = true;
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
        _isCacheDirty = true;
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

        _isCacheDirty = true;
    }

    public void AppendActiveNumbersToGrid()
    {
        var numbersToAdd = GetAllActiveCellData()
            .Select(cell => cell.Number)
            .ToList();
        if (numbersToAdd.Count == 0) return;

        var lastLine = Cells.LastOrDefault();
        if (lastLine != null)
        {
            for (var i = lastLine.Count - 1; i >= 0; i--)
            {
                if (lastLine[i].IsActive) break;

                var cellToRemove = lastLine[i];
                _cellDataMap.Remove(cellToRemove.Id);
                OnCellRemoved?.Invoke(cellToRemove.Id);
                lastLine.RemoveAt(i);
            }
        }

        var lastLineIndex = Cells.Count > 0 ? Cells.Max(l => l.First().Line) : -1;
        var currentLine = Cells.LastOrDefault();

        foreach (var number in numbersToAdd)
        {
            if (currentLine == null || currentLine.Count >= GameConstants.QuantityByWidth)
            {
                lastLineIndex++;
                currentLine = new List<CellData>();
                Cells.Add(currentLine);
            }

            var newCellData = new CellData(number, lastLineIndex, currentLine.Count);
            currentLine.Add(newCellData);
            _cellDataMap[newCellData.Id] = newCellData;
            OnCellAdded?.Invoke(newCellData);
        }

        _isCacheDirty = true;
    }

    public List<int> GetNumbersForTopLine(int numberLine)
    {
        var topNumbers = new List<int>(new int[GameConstants.QuantityByWidth]);
        if (numberLine < 0) return topNumbers;
        numberLine = Mathf.Min(numberLine, Cells.Count);
        for (var col = 0; col < GameConstants.QuantityByWidth; col++)
        {
            for (var line = numberLine - 1; line >= 0; line--)
            {
                var cellData = Cells[line].FirstOrDefault(d => d.Column == col);
                if (cellData is { IsActive: true })
                {
                    topNumbers[col] = cellData.Number;
                    break;
                }
            }
        }

        return topNumbers;
    }
}