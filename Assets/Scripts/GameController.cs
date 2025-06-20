using System;
using System.Collections.Generic;
using System.Linq;

public class GameController
{
    public event Action<Guid, Guid> OnMatchFound;
    public event Action OnInvalidMatch;
    public event Action<Guid> OnCellSelected;
    public event Action<Guid> OnCellDeselected;

    private readonly GridModel _gridModel;
    private readonly CalculatingMatches _calculatingMatches;
    private const int InitialQuantityByHeight = 5;
    private Guid? _firstSelectedCellId;

    public GameController(GridModel gridModel, CalculatingMatches calculatingMatches)
    {
        _gridModel = gridModel;
        _calculatingMatches = calculatingMatches;
    }

    public void AddExistingNumbersAsNewLines()
    {
        _gridModel.AppendActiveNumbersToGrid();
    }

    public void HandleCellSelection(Guid cellId)
    {
        var data = _gridModel.GetCellDataById(cellId);
        if (data is not { IsActive: true }) return;
        if (_firstSelectedCellId == null)
        {
            _firstSelectedCellId = cellId;
            OnCellSelected?.Invoke(cellId);
        }
        else if (_firstSelectedCellId == cellId)
        {
            OnCellDeselected?.Invoke(cellId);
            _firstSelectedCellId = null;
        }
        else
        {
            var firstData = _gridModel.GetCellDataById(_firstSelectedCellId.Value);
            OnCellDeselected?.Invoke(_firstSelectedCellId.Value);
            if (firstData != null && _calculatingMatches.IsAValidMatch(firstData, data))
            {
                ProcessValidMatch(firstData, data);
            }
            else
            {
                OnInvalidMatch?.Invoke();
            }

            _firstSelectedCellId = null;
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
        if (line1 != line2 && _gridModel.IsLineEmpty(line2)) linesToRemove.Add(line2); // Проверка, чтобы не добавлять одну и ту же линию дважды
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
        if (_firstSelectedCellId.HasValue)
        {
            OnCellDeselected?.Invoke(_firstSelectedCellId.Value);
            _firstSelectedCellId = null;
        }

        _gridModel.ClearField();
        for (var i = 0; i < InitialQuantityByHeight; i++)
        {
            _gridModel.CreateLine(i);
        }
    }
}