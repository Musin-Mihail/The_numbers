using System.Collections.Generic;
using Interfaces;
using Model;
using UnityEngine;

namespace DataProviders
{
    /// <summary>
    /// Предоставляет данные о состоянии игровой сетки для других систем,
    /// таких как валидатор совпадений.
    /// </summary>
    public class GridDataProvider : IGridDataProvider
    {
        private readonly GridModel _gridModel;

        /// <summary>
        /// Инициализирует новый экземпляр провайдера данных с ссылкой на модель сетки.
        /// </summary>
        /// <param name="gridModel">Модель игровой сетки.</param>
        public GridDataProvider(GridModel gridModel)
        {
            _gridModel = gridModel;
        }

        /// <summary>
        /// Возвращает список всех активных ячеек на сетке.
        /// </summary>
        /// <returns>Список данных активных ячеек.</returns>
        public List<CellData> GetAllActiveCellData()
        {
            return _gridModel.GetAllActiveCellData();
        }

        /// <summary>
        /// Проверяет, находятся ли две ячейки на одной линии или в одном столбце без активных ячеек между ними.
        /// Эта версия оптимизирована для работы с плотной (не разреженной) сеткой.
        /// </summary>
        /// <param name="firstCell">Первая ячейка.</param>
        /// <param name="secondCell">Вторая ячейка.</param>
        /// <returns>True, если между ячейками нет препятствий.</returns>
        public bool AreCellsOnSameLineOrColumnWithoutGaps(CellData firstCell, CellData secondCell)
        {
            var onSameLine = firstCell.Line == secondCell.Line;
            var onSameColumn = firstCell.Column == secondCell.Column;
            if (!onSameLine && !onSameColumn) return false;
            var grid = _gridModel.Cells;
            if (onSameLine)
            {
                var line = firstCell.Line;
                if (line < 0 || line >= grid.Count) return false;
                var startCol = Mathf.Min(firstCell.Column, secondCell.Column) + 1;
                var endCol = Mathf.Max(firstCell.Column, secondCell.Column);
                for (var column = startCol; column < endCol; column++)
                {
                    if (grid[line][column].IsActive)
                    {
                        return false;
                    }
                }

                return true;
            }

            var col = firstCell.Column;
            var startLine = Mathf.Min(firstCell.Line, secondCell.Line) + 1;
            var endLine = Mathf.Max(firstCell.Line, secondCell.Line);
            for (var line = startLine; line < endLine; line++)
            {
                if (line < 0 || line >= grid.Count) continue;
                if (grid[line][col].IsActive)
                {
                    return false;
                }
            }

            return true;
        }
    }
}