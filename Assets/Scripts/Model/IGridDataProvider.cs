using System.Collections.Generic;

namespace Model
{
    public interface IGridDataProvider
    {
        bool AreCellsOnSameLineOrColumnWithoutGaps(CellData firstCell, CellData secondCell);
        List<CellData> GetAllActiveCellData();
    }
}