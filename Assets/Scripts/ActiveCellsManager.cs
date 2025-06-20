using System;
using System.Collections.Generic;
using UnityEngine;

public class ActiveCellsManager : IGridDataProvider
{
    private readonly List<CellData> _activeCells = new();
    private readonly GridModel _gridModel;

    public ActiveCellsManager(GridModel gridModel)
    {
        _gridModel = gridModel;
        _gridModel.OnCellActiveStateChanged += HandleCellActiveStateChange;
        _gridModel.OnCellRemoved += HandleCellRemoved;
        _gridModel.OnGridCleared += HandleGridCleared;
    }

    private void HandleCellActiveStateChange(CellData cell, bool isActive)
    {
        if (isActive)
        {
            if (!_activeCells.Contains(cell))
            {
                _activeCells.Add(cell);
            }
        }
        else
        {
            _activeCells.Remove(cell);
        }
    }

    private void HandleCellRemoved(Guid cellId)
    {
        _activeCells.RemoveAll(cell => cell.Id == cellId);
    }

    private void HandleGridCleared()
    {
        _activeCells.Clear();
    }

    public List<CellData> GetAllActiveCellData()
    {
        return _activeCells;
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

            return !_activeCells.Exists(cell => cell.Line == line && cell.Column > startCol && cell.Column < endCol);
        }

        var col = firstCell.Column;
        var startLine = Mathf.Min(firstCell.Line, secondCell.Line);
        var endLine = Mathf.Max(firstCell.Line, secondCell.Line);

        return !_activeCells.Exists(cell => cell.Column == col && cell.Line > startLine && cell.Line < endLine);
    }

    public void UnsubscribeEvents()
    {
        _gridModel.OnCellActiveStateChanged -= HandleCellActiveStateChange;
        _gridModel.OnCellRemoved -= HandleCellRemoved;
        _gridModel.OnGridCleared -= HandleGridCleared;
    }
}