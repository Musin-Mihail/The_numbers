using Model;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Проверяет, является ли пара ячеек допустимым совпадением согласно правилам игры.
    /// </summary>
    public class MatchValidator
    {
        private readonly IGridDataProvider _gridDataProvider;

        public MatchValidator(IGridDataProvider gridDataProvider)
        {
            _gridDataProvider = gridDataProvider;
        }

        /// <summary>
        /// Определяет, можно ли составить пару из двух ячеек.
        /// </summary>
        /// <param name="firstCell">Данные первой ячейки.</param>
        /// <param name="secondCell">Данные второй ячейки.</param>
        /// <returns>True, если пара является допустимой.</returns>
        public bool IsAValidMatch(CellData firstCell, CellData secondCell)
        {
            // Правило 1: Числа должны быть одинаковыми или их сумма должна равняться 10.
            if (firstCell.Number != secondCell.Number && firstCell.Number + secondCell.Number != 10)
            {
                return false;
            }

            // Правило 2: Ячейки находятся на одной линии или в одном столбце без препятствий.
            if (_gridDataProvider.AreCellsOnSameLineOrColumnWithoutGaps(firstCell, secondCell))
            {
                return true;
            }

            // Правило 3: Ячейки являются соседними в последовательности всех активных ячеек.
            var activeCells = _gridDataProvider.GetAllActiveCellData();
            var activeCellCount = activeCells.Count;

            var firstIndex = activeCells.IndexOf(firstCell);
            var secondIndex = activeCells.IndexOf(secondCell);

            if (firstIndex == -1 || secondIndex == -1) return false;

            if (Mathf.Abs(firstIndex - secondIndex) == 1)
            {
                return true;
            }

            // Правило 4: Одна ячейка первая, а другая последняя в списке всех активных ячеек.
            return activeCellCount > 1 &&
                   ((firstIndex == 0 && secondIndex == activeCellCount - 1) ||
                    (secondIndex == 0 && firstIndex == activeCellCount - 1));
        }
    }
}