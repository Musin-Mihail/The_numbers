using Model;
using UnityEngine;

namespace Gameplay
{
    public class MatchValidator
    {
        private readonly IGridDataProvider _gridDataProvider;

        public MatchValidator(IGridDataProvider gridDataProvider)
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

            var activeCells = _gridDataProvider.GetAllActiveCellData();
            var activeCellCount = activeCells.Count;

            var firstIndex = activeCells.IndexOf(firstCell);
            var secondIndex = activeCells.IndexOf(secondCell);

            if (firstIndex == -1 || secondIndex == -1) return false;

            if (Mathf.Abs(firstIndex - secondIndex) == 1)
            {
                return true;
            }

            return activeCellCount > 1 &&
                   ((firstIndex == 0 && secondIndex == activeCellCount - 1) ||
                    (secondIndex == 0 && firstIndex == activeCellCount - 1));
        }
    }
}