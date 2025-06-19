using UnityEngine;

[RequireComponent(typeof(GeneratingPlayingField))]
public class CalculatingMatches : MonoBehaviour
{
    private Cell _firstCell;
    private Cell _secondCell;
    private GridModel _gridModel;
    private GeneratingPlayingField _fieldView;

    private void OnEnable()
    {
        ActionBus.OnSelectingCell += HandleCellSelection;
    }

    private void OnDisable()
    {
        ActionBus.OnSelectingCell -= HandleCellSelection;
    }

    public void Initialize(GeneratingPlayingField fieldView, GridModel gridModel)
    {
        _fieldView = fieldView;
        _gridModel = gridModel;
    }

    private void HandleCellSelection(Cell cell)
    {
        if (_gridModel == null) return;

        if (!_firstCell)
        {
            _firstCell = cell;
        }
        else if (_firstCell == cell)
        {
            _firstCell.OnDeselectingCell();
            _firstCell = null;
        }
        else
        {
            _secondCell = cell;
        }

        if (!_firstCell || !_secondCell) return;

        if (IsAValidMatch())
        {
            _firstCell.DisableCell();
            _secondCell.DisableCell();
            ActionBus.CheckLines(_firstCell.line, _secondCell.line);

            if (_fieldView) _fieldView.NotifyMatchOccured();
        }
        else
        {
            _firstCell.OnDeselectingCell();
            _secondCell.OnDeselectingCell();
        }

        _firstCell = null;
        _secondCell = null;
    }

    private bool IsAValidMatch()
    {
        if (_firstCell.number != _secondCell.number && _firstCell.number + _secondCell.number != 10)
        {
            return false;
        }

        if (_gridModel.AreCellsOnSameLineOrColumnWithoutGaps(_firstCell, _secondCell))
        {
            return true;
        }

        var activeCells = _gridModel.GetAllActiveCells();
        var firstIndex = activeCells.IndexOf(_firstCell);
        var secondIndex = activeCells.IndexOf(_secondCell);

        if (firstIndex == -1 || secondIndex == -1) return false;

        if (Mathf.Abs(firstIndex - secondIndex) == 1)
        {
            return true;
        }

        if ((firstIndex == 0 && secondIndex == activeCells.Count - 1) || (secondIndex == 0 && firstIndex == activeCells.Count - 1))
        {
            return true;
        }

        return false;
    }
}