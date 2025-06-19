using UnityEngine;

public class CalculatingMatches
{
    private GridModel _gridModel;

    public CalculatingMatches(GridModel gridModel)
    {
        _gridModel = gridModel;
    }

    public bool IsAValidMatch(Cell firstCell, Cell secondCell)
    {
        if (firstCell.number != secondCell.number && firstCell.number + secondCell.number != 10)
        {
            return false;
        }

        if (_gridModel.AreCellsOnSameLineOrColumnWithoutGaps(firstCell, secondCell))
        {
            return true;
        }

        var activeCells = _gridModel.GetAllActiveCells();
        var firstIndex = activeCells.IndexOf(firstCell);
        var secondIndex = activeCells.IndexOf(secondCell);

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