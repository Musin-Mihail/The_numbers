using UnityEngine;

public class CalculatingMatches : MonoBehaviour
{
    private Cell _firstCell;
    private Cell _secondCell;

    private void OnEnable()
    {
        ActionBus.OnSelectingCell += CheckNumber;
    }

    private void OnDisable()
    {
        ActionBus.OnSelectingCell -= CheckNumber;
    }

    private void CheckNumber(Cell cell)
    {
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
        if ((_firstCell.number == _secondCell.number || _firstCell.number + _secondCell.number == 10) && AreDifferencesWithinRangeAndNotDiagonal())
        {
            _firstCell.DisableCell();
            _secondCell.DisableCell();
            ActionBus.CheckLines(_firstCell.line, _secondCell.line);
        }
        else
        {
            _firstCell.OnDeselectingCell();
            _secondCell.OnDeselectingCell();
        }

        _firstCell = null;
        _secondCell = null;
    }

    private bool AreDifferencesWithinRangeAndNotDiagonal()
    {
        var widthDifference = _firstCell.line - _secondCell.line;
        var heightDifference = _firstCell.column - _secondCell.column;
        var isWidthDifferenceValid = widthDifference is >= -1 and <= 1;
        var isHeightDifferenceValid = heightDifference is >= -1 and <= 1;
        if (!isWidthDifferenceValid || !isHeightDifferenceValid)
        {
            return false;
        }

        var isNotDiagonal = (widthDifference == 0 || heightDifference == 0);
        return isNotDiagonal;
    }
}