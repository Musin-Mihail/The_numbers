using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Этот класс отвечает за основную логику игры: проверку и сопоставление выбранных ячеек
/// на основе правил.
/// </summary>
public class CalculatingMatches : MonoBehaviour
{
    private Cell _firstCell;
    private Cell _secondCell;

    private void OnEnable()
    {
        ActionBus.OnSelectingCell += HandleCellSelection;
    }

    private void OnDisable()
    {
        ActionBus.OnSelectingCell -= HandleCellSelection;
    }

    /// <summary>
    /// Обрабатывает выбор ячейки пользователем.
    /// </summary>
    private void HandleCellSelection(Cell cell)
    {
        if (!_firstCell)
        {
            _firstCell = cell;
        }
        else if (_firstCell == cell)
        {
            _firstCell.OnDeselectingCell();
            _firstCell = null;
        }
        else
        {
            _secondCell = cell;
        }

        if (!_firstCell || !_secondCell) return;
        if (IsAValidMatch())
        {
            _firstCell.DisableCell();
            _secondCell.DisableCell();
            ActionBus.CheckLines(_firstCell.line, _secondCell.line);
        }
        else
        {
            _firstCell.OnDeselectingCell();
            _secondCell.OnDeselectingCell();
        }

        _firstCell = null;
        _secondCell = null;
    }

    /// <summary>
    /// Комплексная проверка, объединяющая все правила игры.
    /// </summary>
    /// <returns>True, если пара является допустимой.</returns>
    private bool IsAValidMatch()
    {
        if (_firstCell.number != _secondCell.number && _firstCell.number + _secondCell.number != 10)
        {
            return false;
        }

        if (AreOnSameLineOrColumnWithGaps())
        {
            return true;
        }

        var activeCells = GetAllActiveCells();
        var firstIndex = activeCells.IndexOf(_firstCell);
        var secondIndex = activeCells.IndexOf(_secondCell);

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

    /// <summary>
    /// ПРАВИЛО 1 и 4: Проверяет, находятся ли ячейки на одной прямой (горизонтальной или вертикальной)
    /// и есть ли между ними только пустые (неактивные) ячейки.
    /// </summary>
    private bool AreOnSameLineOrColumnWithGaps()
    {
        var onSameLine = _firstCell.line == _secondCell.line;
        var onSameColumn = _firstCell.column == _secondCell.column;

        if (!onSameLine && !onSameColumn)
        {
            return false;
        }

        var cells = GeneratingPlayingField.Instance.Cells;

        if (onSameLine)
        {
            var line = _firstCell.line;
            var startCol = Mathf.Min(_firstCell.column, _secondCell.column);
            var endCol = Mathf.Max(_firstCell.column, _secondCell.column);
            for (var c = startCol + 1; c < endCol; c++)
            {
                if (cells[line][c].gameObject.activeSelf)
                {
                    return false;
                }
            }
        }
        else
        {
            var col = _firstCell.column;
            var startLine = Mathf.Min(_firstCell.line, _secondCell.line);
            var endLine = Mathf.Max(_firstCell.line, _secondCell.line);
            for (var l = startLine + 1; l < endLine; l++)
            {
                if (l < cells.Count && col < cells[l].Count && cells[l][col].gameObject.activeSelf)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Вспомогательный метод для получения одномерного списка всех активных ячеек на поле,
    /// следующих в порядке их чтения (слева направо, сверху вниз).
    /// </summary>
    /// <returns>Список активных ячеек.</returns>
    private List<Cell> GetAllActiveCells()
    {
        return GeneratingPlayingField.Instance.Cells
            .SelectMany(line => line)
            .Where(cell => cell && cell.gameObject.activeSelf)
            .ToList();
    }
}