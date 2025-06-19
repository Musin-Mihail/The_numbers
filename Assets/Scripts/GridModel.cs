using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridModel : IGridDataProvider
{
    public List<List<CellData>> Cells { get; } = new();
    private const int QuantityByWidth = 10;
    private readonly GeneratingPlayingField _view;

    public GridModel(GeneratingPlayingField view)
    {
        _view = view;
    }

    public List<Cell> GetAllActiveCells()
    {
        return Cells
            .SelectMany(line => line)
            .Where(data => data != null && data.IsActive)
            .Select(data => _view.GetCellView(data.Id))
            .Where(cell => cell != null)
            .ToList();
    }

    public bool AreCellsOnSameLineOrColumnWithoutGaps(Cell firstCell, Cell secondCell)
    {
        var onSameLine = firstCell.line == secondCell.line;
        var onSameColumn = firstCell.column == secondCell.column;

        if (!onSameLine && !onSameColumn) return false;

        var allCellsData = Cells.SelectMany(list => list).ToList();

        if (onSameLine)
        {
            var line = firstCell.line;
            var startCol = Mathf.Min(firstCell.column, secondCell.column);
            var endCol = Mathf.Max(firstCell.column, secondCell.column);
            for (var c = startCol + 1; c < endCol; c++)
            {
                var data = allCellsData.FirstOrDefault(d => d.Line == line && d.Column == c);
                if (data != null && data.IsActive) return false;
            }
        }
        else // onSameColumn
        {
            var col = firstCell.column;
            var startLine = Mathf.Min(firstCell.line, secondCell.line);
            var endLine = Mathf.Max(firstCell.line, secondCell.line);
            for (var l = startLine + 1; l < endLine; l++)
            {
                var data = allCellsData.FirstOrDefault(d => d.Line == l && d.Column == col);
                if (data != null && data.IsActive) return false;
            }
        }

        return true;
    }

    public CellData GetCellDataById(Guid id)
    {
        return Cells.SelectMany(line => line).FirstOrDefault(d => d.Id == id);
    }

    public void ClearField()
    {
        Cells.Clear();
    }

    public void CreateLine(int lineIndex)
    {
        var newLine = new List<CellData>();
        for (var i = 0; i < QuantityByWidth; i++)
        {
            newLine.Add(new CellData(Random.Range(1, 10), lineIndex, i));
        }

        Cells.Add(newLine);
    }

    public bool IsLineEmpty(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Cells.Count) return false;
        return Cells[lineIndex].All(cellData => !cellData.IsActive);
    }

    public void RemoveLine(int numberLine)
    {
        if (numberLine < 0 || numberLine >= Cells.Count) return;
        Cells.RemoveAt(numberLine);
        for (int i = 0; i < Cells.Count; i++)
        {
            foreach (var cell in Cells[i])
            {
                cell.Line = i;
            }
        }
    }

    // ИСПРАВЛЕНО: Возвращена логика очистки неактивных ячеек в конце последней строки.
    // Метод переименован для ясности.
    public void AppendActiveNumbersToGrid()
    {
        var activeCellsData = Cells.SelectMany(line => line).Where(d => d.IsActive).ToList();
        var numbersToAdd = activeCellsData.Select(cell => cell.Number).ToList();

        if (numbersToAdd.Count == 0) return;

        int lastLineIndex = Cells.Count > 0 ? Cells.Max(l => l.First().Line) : -1;

        List<CellData> currentLine = Cells.LastOrDefault();

        if (currentLine == null)
        {
            lastLineIndex = 0;
            currentLine = new List<CellData>();
            Cells.Add(currentLine);
        }
        else
        {
            // Восстановленная логика: ищем с конца последней строки
            // и удаляем неактивные ячейки, пока не встретим активную.
            for (int i = currentLine.Count - 1; i >= 0; i--)
            {
                if (currentLine[i].IsActive)
                {
                    break; // Нашли последнюю активную, останавливаемся.
                }

                currentLine.RemoveAt(i); // Удаляем неактивную ячейку с конца.
            }
        }

        foreach (var number in numbersToAdd)
        {
            if (currentLine.Count >= QuantityByWidth)
            {
                lastLineIndex++;
                currentLine = new List<CellData>();
                Cells.Add(currentLine);
            }

            var newCellData = new CellData(number, lastLineIndex, currentLine.Count);
            currentLine.Add(newCellData);
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
                if (l < Cells.Count && i < Cells[l].Count)
                {
                    var cellData = Cells[l][i];
                    if (cellData != null && cellData.IsActive)
                    {
                        activeNumbers.Add(cellData.Number);
                        found = true;
                        break;
                    }
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