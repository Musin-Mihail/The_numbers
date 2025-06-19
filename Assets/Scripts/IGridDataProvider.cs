using System.Collections.Generic;

public interface IGridDataProvider
{
    bool AreCellsOnSameLineOrColumnWithoutGaps(Cell firstCell, Cell secondCell);
    List<Cell> GetAllActiveCells();
}