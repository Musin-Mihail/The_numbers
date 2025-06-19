using UnityEngine;

public class CalculatingMatches
{
    private readonly IGridDataProvider _gridDataProvider;

    public CalculatingMatches(IGridDataProvider gridDataProvider)
    {
        _gridDataProvider = gridDataProvider;
    }

    public bool IsAValidMatch(CellData firstCell, CellData secondCell)
    {
        if (firstCell.Number != secondCell.Number && firstCell.Number + secondCell.Number != 10)
        {
            return false;
        }

        if (_gridDataProvider.AreCellsOnSameLineOrColumnWithoutGaps(firstCell, secondCell))
        {
            return true;
        }

        var firstIndex = _gridDataProvider.GetIndexOfActiveCell(firstCell);
        var secondIndex = _gridDataProvider.GetIndexOfActiveCell(secondCell);
        if (firstIndex == -1 || secondIndex == -1) return false;
        if (Mathf.Abs(firstIndex - secondIndex) == 1)
        {
            return true;
        }

        var activeCellCount = _gridDataProvider.GetAllActiveCellData().Count;
        if (activeCellCount > 1 &&
            ((firstIndex == 0 && secondIndex == activeCellCount - 1) ||
             (secondIndex == 0 && firstIndex == activeCellCount - 1)))
        {
            return true;
        }

        return false;
    }
}