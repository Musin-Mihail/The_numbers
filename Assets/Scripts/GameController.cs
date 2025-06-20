using System;
using System.Collections.Generic;
using System.Linq;

public class GameController
{
    public event Action<Guid, Guid> OnMatchFound;
    public event Action OnInvalidMatch;
    private readonly GridModel _gridModel;
    private readonly CalculatingMatches _calculatingMatches;
    private const int InitialQuantityByHeight = 5;

    public GameController(GridModel gridModel, CalculatingMatches calculatingMatches)
    {
        _gridModel = gridModel;
        _calculatingMatches = calculatingMatches;
    }

    public void AddExistingNumbersAsNewLines()
    {
        _gridModel.AppendActiveNumbersToGrid();
    }

    public void AttemptMatch(Guid firstCellId, Guid secondCellId)
    {
        var firstData = _gridModel.GetCellDataById(firstCellId);
        var secondData = _gridModel.GetCellDataById(secondCellId);

        if (firstData != null && secondData != null && _calculatingMatches.IsAValidMatch(firstData, secondData))
        {
            ProcessValidMatch(firstData, secondData);
        }
        else
        {
            OnInvalidMatch?.Invoke();
        }
    }

    private void ProcessValidMatch(CellData data1, CellData data2)
    {
        OnMatchFound?.Invoke(data1.Id, data2.Id);
        _gridModel.SetCellActiveState(data1, false);
        _gridModel.SetCellActiveState(data2, false);
        CheckAndRemoveEmptyLines(data1.Line, data2.Line);
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

    public void StartNewGame()
    {
        _gridModel.ClearField();
        for (var i = 0; i < InitialQuantityByHeight; i++)
        {
            _gridModel.CreateLine(i);
        }
    }
}