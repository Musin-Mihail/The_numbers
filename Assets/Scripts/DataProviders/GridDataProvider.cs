using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace DataProviders
{
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
            var grid = _gridModel.Cells;
            if (onSameLine)
            {
                var line = firstCell.Line;
                if (line < 0 || line >= grid.Count) return false;
                var startCol = Mathf.Min(firstCell.Column, secondCell.Column) + 1;
                var endCol = Mathf.Max(firstCell.Column, secondCell.Column);
                for (var column = startCol; column < endCol; column++)
                {
                    var cell = grid[line].FirstOrDefault(c => c.Column == column);
                    if (cell is { IsActive: true })
                    {
                        return false;
                    }
                }

                return true;
            }

            var col = firstCell.Column;
            var startLine = Mathf.Min(firstCell.Line, secondCell.Line) + 1;
            var endLine = Mathf.Max(firstCell.Line, secondCell.Line);

            for (var line = startLine; line < endLine; line++)
            {
                if (line < 0 || line >= grid.Count) continue;

                var cell = grid[line].FirstOrDefault(c => c.Column == col);
                if (cell is { IsActive: true })
                {
                    return false;
                }
            }

            return true;
        }

        public CellData GetCellData(int line, int column)
        {
            if (line < 0 || line >= _gridModel.Cells.Count) return null;
            return _gridModel.Cells[line].FirstOrDefault(c => c.Column == column);
        }

        public bool IsLineEmpty(int lineIndex)
        {
            return _gridModel.IsLineEmpty(lineIndex);
        }

        public int GetLineCount()
        {
            return _gridModel.Cells.Count;
        }

        public CellData FindFirstActiveCellInDirection(int startLine, int startCol, int dLine, int dCol)
        {
            var currentLine = startLine + dLine;
            var currentCol = startCol + dCol;
            var grid = _gridModel.Cells;

            if (dCol == 0 && dLine != 0)
            {
                while (currentLine >= 0 && currentLine < grid.Count)
                {
                    var cell = GetCellData(currentLine, currentCol);
                    if (cell is { IsActive: true })
                    {
                        return cell;
                    }

                    currentLine += dLine;
                }
            }
            else if (dLine == 0 && dCol != 0)
            {
                if (startLine < 0 || startLine >= grid.Count) return null;

                while (currentCol is >= 0 and < GameConstants.QuantityByWidth)
                {
                    var cell = GetCellData(startLine, currentCol);
                    if (cell is { IsActive: true })
                    {
                        return cell;
                    }

                    currentCol += dCol;
                }
            }

            return null;
        }
    }
}