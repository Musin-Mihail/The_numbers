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
        var onSameLine = _firstCell.line == _secondCell.line;
        var onSameColumn = _firstCell.column == _secondCell.column;

        if (!onSameLine && !onSameColumn)
        {
            return false;
        }

        var cells = GeneratingPlayingField.Instance.Cells;

        if (onSameLine)
        {
            var line = _firstCell.line;
            if (line >= cells.Count) return false;

            var startCol = Mathf.Min(_firstCell.column, _secondCell.column);
            var endCol = Mathf.Max(_firstCell.column, _secondCell.column);

            if (endCol >= cells[line].Count) return false;

            for (var c = startCol + 1; c < endCol; c++)
            {
                if (cells[line][c].gameObject.activeSelf)
                {
                    return false;
                }
            }
        }
        else
        {
            var col = _firstCell.column;
            var startLine = Mathf.Min(_firstCell.line, _secondCell.line);
            var endLine = Mathf.Max(_firstCell.line, _secondCell.line);

            for (var l = startLine + 1; l < endLine; l++)
            {
                if (l >= cells.Count || col >= cells[l].Count)
                {
                    return false;
                }

                if (cells[l][col].gameObject.activeSelf)
                {
                    return false;
                }
            }
        }

        return true;
    }
}