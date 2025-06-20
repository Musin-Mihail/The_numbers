using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridDataProvider : IGridDataProvider
{
    private readonly GridModel _gridModel;

    public GridDataProvider(GridModel gridModel)
    {
        _gridModel = gridModel;
    }

    public List<CellData> GetAllActiveCellData()
    {
        return _gridModel.GetAllActiveCellData();
    }

    public bool AreCellsOnSameLineOrColumnWithoutGaps(CellData firstCell, CellData secondCell)
    {
        var onSameLine = firstCell.Line == secondCell.Line;
        var onSameColumn = firstCell.Column == secondCell.Column;
        if (!onSameLine && !onSameColumn) return false;
        var allActiveCells = GetAllActiveCellData();
        if (onSameLine)
        {
            var line = firstCell.Line;
            var startCol = Mathf.Min(firstCell.Column, secondCell.Column);
            var endCol = Mathf.Max(firstCell.Column, secondCell.Column);
            return !allActiveCells.Any(cell => cell.Line == line && cell.Column > startCol && cell.Column < endCol);
        }

        // else (onSameColumn)
        var col = firstCell.Column;
        var startLine = Mathf.Min(firstCell.Line, secondCell.Line);
        var endLine = Mathf.Max(firstCell.Line, secondCell.Line);
        return !allActiveCells.Any(cell => cell.Column == col && cell.Line > startLine && cell.Line < endLine);
    }
}