using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridModel : IGridDataProvider
{
    public List<List<CellData>> Cells { get; } = new();
    private readonly Dictionary<Guid, CellData> _cellDataMap = new();
    private List<Cell> _activeCellsCache = new();
    private bool _isCacheDirty = true;
    private readonly GeneratingPlayingField _view;

    public GridModel(GeneratingPlayingField view)
    {
        _view = view;
    }

    public List<CellData> GetAllCellData() => _cellDataMap.Values.ToList();

    public List<Cell> GetAllActiveCells()
    {
        if (_isCacheDirty)
        {
            _activeCellsCache = _cellDataMap.Values
                .Where(data => data.IsActive)
                .OrderBy(data => data.Line)
                .ThenBy(data => data.Column)
                .Select(data => _view.GetCellView(data.Id))
                .Where(cell => cell)
                .ToList();
            _isCacheDirty = false;
        }

        return _activeCellsCache;
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

            var lineData = Cells[line];
            for (var c = startCol + 1; c < endCol; c++)
            {
                var data = lineData.FirstOrDefault(d => d.Column == c);
                if (data is { IsActive: true }) return false;
            }
        }
        else // onSameColumn
        {
            var col = firstCell.column;
            var startLine = Mathf.Min(firstCell.line, secondCell.line);
            var endLine = Mathf.Max(firstCell.line, secondCell.line);
            for (var l = startLine + 1; l < endLine; l++)
            {
                if (l < Cells.Count)
                {
                    var data = Cells[l].FirstOrDefault(d => d.Column == col);
                    if (data is { IsActive: true }) return false; // Найдена преграда
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
    }

    public void ClearField()
    {
        Cells.Clear();
        _cellDataMap.Clear();
        _isCacheDirty = true;
    }

    public void CreateLine(int lineIndex)
    {
        var newLine = new List<CellData>();
        for (var i = 0; i < GameConstants.QuantityByWidth; i++)
        {
            var cellData = new CellData(Random.Range(1, 10), lineIndex, i);
            newLine.Add(cellData);
            _cellDataMap[cellData.Id] = cellData;
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
        }

        Cells.RemoveAt(lineIndex);
        for (var i = lineIndex; i < Cells.Count; i++)
        {
            foreach (var cell in Cells[i])
            {
                cell.Line = i;
            }
        }

        _isCacheDirty = true;
    }

    public void AppendActiveNumbersToGrid()
    {
        var numbersToAdd = _cellDataMap.Values
            .Where(d => d.IsActive)
            .OrderBy(d => d.Line)
            .ThenBy(d => d.Column)
            .Select(cell => cell.Number)
            .ToList();
        if (numbersToAdd.Count == 0) return;
        var lastLineIndex = Cells.Count > 0 ? Cells.Max(l => l.First().Line) : -1;
        var currentLine = Cells.LastOrDefault();
        if (currentLine != null)
        {
            for (var i = currentLine.Count - 1; i >= 0; i--)
            {
                if (currentLine[i].IsActive) break;
                _cellDataMap.Remove(currentLine[i].Id);
                currentLine.RemoveAt(i);
            }
        }

        foreach (var number in numbersToAdd)
        {
            if (currentLine is not { Count: < GameConstants.QuantityByWidth })
            {
                lastLineIndex++;
                currentLine = new List<CellData>();
                Cells.Add(currentLine);
            }

            var newCellData = new CellData(number, lastLineIndex, currentLine.Count);
            currentLine.Add(newCellData);
            _cellDataMap[newCellData.Id] = newCellData;
        }

        _isCacheDirty = true;
    }

    public List<int> GetNumbersForTopLine(int numberLine)
    {
        var topNumbers = new List<int>(new int[GameConstants.QuantityByWidth]); // Инициализируем нулями
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