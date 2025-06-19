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
            _secondCell = cell;
            _secondCell.SetSelected(true);
        }

        if (!_firstCell || !_secondCell) return;

        if (_calculatingMatches.IsAValidMatch(_firstCell, _secondCell))
        {
            OnMatchFound?.Invoke(_firstCell, _secondCell);

            var line1 = _firstCell.line;
            var line2 = _secondCell.line;

            _firstCell.DisableCell();
            _secondCell.DisableCell();

            CheckAndRemoveEmptyLines(line1, line2);
        }
        else
        {
            OnInvalidMatch?.Invoke();
        }

        _firstCell = null;
        _secondCell = null;
    }

    private void CheckAndRemoveEmptyLines(int line1, int line2)
    {
        var linesToRemove = new List<int>();
        if (_gridModel.IsLineEmpty(line1)) linesToRemove.Add(line1);
        if (line1 != line2 && _gridModel.IsLineEmpty(line2)) linesToRemove.Add(line2);

        if (linesToRemove.Count > 0)
        {
            linesToRemove.Sort((a, b) => b.CompareTo(a));

            foreach (var lineIndex in linesToRemove)
            {
                _gridModel.RemoveLine(lineIndex);
            }

            OnGridChanged?.Invoke();
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
            _gridModel.CreateLine();
        }

        OnGridChanged?.Invoke();
    }

    public void AddExistingNumbersAsNewLines()
    {
        var numbersToAdd = _gridModel.GetAllActiveCells()
            .Select(cell => cell.number)
            .ToList();

        if (numbersToAdd.Count == 0) return;

        _gridModel.AddNumbersAsNewLines(numbersToAdd);
        OnGridChanged?.Invoke();
    }
}