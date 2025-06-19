using System.Collections.Generic;

public interface IGridDataProvider
{
    bool AreCellsOnSameLineOrColumnWithoutGaps(CellData firstCell, CellData secondCell);
    List<CellData> GetAllActiveCellData();
}