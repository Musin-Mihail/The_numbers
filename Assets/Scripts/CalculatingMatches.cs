using UnityEngine;

public class CalculatingMatches
{
    private readonly IGridDataProvider _gridDataProvider;

    public CalculatingMatches(IGridDataProvider gridDataProvider)
    {
        _gridDataProvider = gridDataProvider;
    }

    public bool IsAValidMatch(Cell firstCell, Cell secondCell)
    {
        if (firstCell.Number != secondCell.Number && firstCell.Number + secondCell.Number != 10)
        {
            return false;
        }

        if (_gridDataProvider.AreCellsOnSameLineOrColumnWithoutGaps(firstCell, secondCell))
        {
            return true;
        }

        var activeCells = _gridDataProvider.GetAllActiveCells();
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