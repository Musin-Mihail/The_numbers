using System;
using System.Collections.Generic;
using System.Linq;

public class GameController
{
    public event Action<Cell, Cell> OnMatchFound;
    public event Action OnInvalidMatch;
    public event Action OnGridChanged;

    private readonly GridModel _gridModel;
    private readonly CalculatingMatches _calculatingMatches;
    private const int InitialQuantityByHeight = 5;
    private Cell _firstCell;

    public GameController(GridModel gridModel, CalculatingMatches calculatingMatches)
    {
        _gridModel = gridModel;
        _calculatingMatches = calculatingMatches;
    }

    public void HandleCellSelection(Cell cell)
    {
        if (!cell.IsActive) return;

        if (!_firstCell)
        {
            _firstCell = cell;
            _firstCell.SetSelected(true);
        }
        else if (_firstCell == cell)
        {
            _firstCell.SetSelected(false);
            _firstCell = null;
        }
        else
        {
            var secondCell = cell;
            _firstCell.SetSelected(false);

            if (_calculatingMatches.IsAValidMatch(_firstCell, secondCell))
            {
                ProcessValidMatch(_firstCell, secondCell);
            }
            else
            {
                OnInvalidMatch?.Invoke();
            }

            _firstCell = null;
        }
    }

    private void ProcessValidMatch(Cell cell1, Cell cell2)
    {
        OnMatchFound?.Invoke(cell1, cell2);
        var data1 = _gridModel.GetCellDataById(cell1.DataId);
        var data2 = _gridModel.GetCellDataById(cell2.DataId);
        if (data1 != null) _gridModel.SetCellActiveState(data1, false);
        if (data2 != null) _gridModel.SetCellActiveState(data2, false);
        CheckAndRemoveEmptyLines(data1.Line, data2.Line);
        OnGridChanged?.Invoke();
    }

    private void CheckAndRemoveEmptyLines(int line1, int line2)
    {
        var linesToRemove = new HashSet<int>();
        if (_gridModel.IsLineEmpty(line1)) linesToRemove.Add(line1);
        if (_gridModel.IsLineEmpty(line2)) linesToRemove.Add(line2);
        if (linesToRemove.Count > 0)
        {
            foreach (var lineIndex in linesToRemove.OrderByDescending(i => i))
            {
                _gridModel.RemoveLine(lineIndex);
            }
        }
    }

    public void StartNewGame()
    {
        _firstCell = null;
        _gridModel.ClearField();
        for (var i = 0; i < InitialQuantityByHeight; i++)
        {
            _gridModel.CreateLine(i);
        }

        OnGridChanged?.Invoke();
    }

    public void AddExistingNumbersAsNewLines()
    {
        _gridModel.AppendActiveNumbersToGrid();
        OnGridChanged?.Invoke();
    }
}