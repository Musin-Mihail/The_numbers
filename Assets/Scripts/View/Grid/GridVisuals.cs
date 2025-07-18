using System;
using System.Collections.Generic;
using Core;
using Model;
using UnityEngine;
using View.UI;

namespace View.Grid
{
    /// <summary>
    /// Управляет всеми визуальными аспектами сетки: подсветкой, выделением,
    /// отображением очков и другими эффектами.
    /// </summary>
    public class GridVisuals
    {
        private readonly Dictionary<Guid, Cell> _cellViewInstances;
        private readonly FloatingScorePool _floatingScorePool;
        private readonly GridModel _gridModel;
        private readonly Color _positiveScoreColor;
        private readonly Color _negativeScoreColor;

        private readonly List<Guid> _hintedCellIds = new();
        public bool HasActiveHints => _hintedCellIds.Count > 0;

        public GridVisuals(Dictionary<Guid, Cell> cellViewInstances, FloatingScorePool floatingScorePool, GridModel gridModel, Color positiveScoreColor, Color negativeScoreColor)
        {
            _cellViewInstances = cellViewInstances;
            _floatingScorePool = floatingScorePool;
            _gridModel = gridModel;
            _positiveScoreColor = positiveScoreColor;
            _negativeScoreColor = negativeScoreColor;
        }

        /// <summary>
        /// Отображает подсказку, подсвечивая две ячейки.
        /// </summary>
        public void ShowHint((Guid firstId, Guid secondId) data)
        {
            ClearHintVisuals();
            _hintedCellIds.Add(data.firstId);
            _hintedCellIds.Add(data.secondId);

            if (_cellViewInstances.TryGetValue(data.firstId, out var firstCell))
            {
                firstCell.SetHighlight(true);
            }

            if (_cellViewInstances.TryGetValue(data.secondId, out var secondCell))
            {
                secondCell.SetHighlight(true);
            }
        }

        /// <summary>
        /// Убирает все визуальные эффекты подсказок.
        /// </summary>
        public void ClearHintVisuals()
        {
            if (_hintedCellIds.Count == 0) return;

            foreach (var id in _hintedCellIds)
            {
                if (_cellViewInstances.TryGetValue(id, out var cell))
                {
                    cell.SetHighlight(false);
                }
            }

            _hintedCellIds.Clear();
        }

        /// <summary>
        /// Устанавливает визуальное состояние выделения для ячейки.
        /// </summary>
        public void SetSelectionVisual(Guid cellId, bool isSelected)
        {
            if (_cellViewInstances.TryGetValue(cellId, out var cellView))
            {
                cellView.SetSelected(isSelected);
            }
        }

        /// <summary>
        /// Показывает всплывающие очки для найденной пары.
        /// </summary>
        public void ShowFloatingScoreForPair(Guid cell1Id, Guid cell2Id, int score, bool isPositive)
        {
            var cell1Data = _gridModel.GetCellDataById(cell1Id);
            var cell2Data = _gridModel.GetCellDataById(cell2Id);
            if (cell1Data == null || cell2Data == null) return;
            
            var pos1Pivot = new Vector2(GameConstants.CellSize * cell1Data.Column + GameConstants.Indent / 2f, -GameConstants.CellSize * cell1Data.Line - GameConstants.Indent / 2f);
            var pos2Pivot = new Vector2(GameConstants.CellSize * cell2Data.Column + GameConstants.Indent / 2f, -GameConstants.CellSize * cell2Data.Line - GameConstants.Indent / 2f);

            var midPoint = (pos1Pivot + pos2Pivot) / 2f;
            var color = isPositive ? _positiveScoreColor : _negativeScoreColor;
            ShowFloatingScore(score, color, midPoint);
        }

        /// <summary>
        /// Показывает всплывающие очки для удаленной линии.
        /// </summary>
        public void ShowFloatingScoreForLine(int lineIndex, int score, bool isPositive)
        {
            var yPos = -lineIndex * GameConstants.CellSize - (GameConstants.CellSize / 2f) - (GameConstants.Indent / 2f);
            var xPos = (GameConstants.QuantityByWidth * GameConstants.CellSize) / 2f;
            var position = new Vector2(xPos, yPos);
            var color = isPositive ? _positiveScoreColor : _negativeScoreColor;
            ShowFloatingScore(score, color, position);
        }

        /// <summary>
        /// Показывает сообщение об очистке доски.
        /// </summary>
        public void ShowBoardClearedMessage(RectTransform viewport)
        {
            if (!_floatingScorePool) return;
            var centerPosition = new Vector2(viewport.rect.width / 2f, -viewport.rect.height / 2f);
            var size = new Vector2(700, 250);
            var adjustedPosition = new Vector2(centerPosition.x - size.x / 2f, centerPosition.y + size.y / 2f);
            var scoreTextInstance = _floatingScorePool.GetScore();
            scoreTextInstance.Show("Множитель +1", _positiveScoreColor, adjustedPosition, size, _floatingScorePool.ReturnScore);
        }

        private void ShowFloatingScore(int score, Color color, Vector2 position)
        {
            if (_floatingScorePool == null) return;
            var scoreTextInstance = _floatingScorePool.GetScore();
            scoreTextInstance.Show(Mathf.Abs(score).ToString(), color, position, new Vector2(107, 107), _floatingScorePool.ReturnScore);
        }
    }
}