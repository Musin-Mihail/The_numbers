using System.Collections.Generic;
using Model;

namespace Interfaces
{
    /// <summary>
    /// Интерфейс для предоставления данных о состоянии игровой сетки.
    /// </summary>
    public interface IGridDataProvider
    {
        /// <summary>
        /// Проверяет, находятся ли две ячейки на одной линии или в одном столбце без активных ячеек между ними.
        /// </summary>
        /// <returns>True, если между ячейками нет препятствий.</returns>
        bool AreCellsOnSameLineOrColumnWithoutGaps(CellData firstCell, CellData secondCell);

        /// <summary>
        /// Возвращает список всех активных ячеек на сетке.
        /// </summary>
        /// <returns>Список данных активных ячеек.</returns>
        List<CellData> GetAllActiveCellData();
    }
}