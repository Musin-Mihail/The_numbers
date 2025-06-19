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
    private Cell _secondCell;

    public GameController(GridModel gridModel, CalculatingMatches calculatingMatches)
    {
        _gridModel = gridModel;
        _calculatingMatches = calculatingMatches;
    }

    public void HandleCellSelection(Cell cell)
    {
        if (!cell.IsActive) return;

        if (_firstCell == null)
        {
            _firstCell = cell;
            _firstCell.SetSelected(true);
        }
        else if (_firstCell == cell)
        {
            _firstCell.SetSelected(false);
            _firstCell = null;
        }
        else // Выбрана вторая ячейка
        {
            _secondCell = cell;

            _firstCell.SetSelected(false);

            if (_calculatingMatches.IsAValidMatch(_firstCell, _secondCell))
            {
                OnMatchFound?.Invoke(_firstCell, _secondCell);

                var data1 = _gridModel.GetCellDataById(_firstCell.DataId);
                var data2 = _gridModel.GetCellDataById(_secondCell.DataId);

                if (data1 != null) data1.SetActive(false);
                if (data2 != null) data2.SetActive(false);

                CheckAndRemoveEmptyLines(data1.Line, data2.Line);
                OnGridChanged?.Invoke();
            }
            else
            {
                OnInvalidMatch?.Invoke();
            }

            _firstCell = null;
            _secondCell = null;
        }
    }

    private void CheckAndRemoveEmptyLines(int line1, int line2)
    {
        var linesToRemove = new HashSet<int>();
        if (_gridModel.IsLineEmpty(line1)) linesToRemove.Add(line1);
        if (line1 != line2 && _gridModel.IsLineEmpty(line2)) linesToRemove.Add(line2);

        if (linesToRemove.Count > 0)
        {
            foreach (var lineIndex in linesToRemove.OrderByDescending(i => i))
            {
                _gridModel.RemoveLine(lineIndex);
            }
        }
    }

    private void ResetSelection()
    {
        _firstCell = null;
        _secondCell = null;
    }

    public void StartNewGame()
    {
        ResetSelection();
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