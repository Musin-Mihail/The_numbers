﻿using Core;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.UI;

namespace View.Grid
{
    /// <summary>
    /// Управляет расположением, размерами и прокруткой элементов сетки.
    /// </summary>
    public class GridLayoutManager
    {
        private readonly RectTransform _contentContainer;
        private readonly ScrollRect _scrollRect;
        private readonly RectTransform _scrollviewContainer;
        private readonly HeaderNumberDisplay _headerNumberDisplay;
        private readonly GridModel _gridModel;
        
        private readonly float _topPaddingValue;
        private readonly float _scrollLoggingThreshold;
        private float _lastLoggedScrollPosition;

        public GridLayoutManager(RectTransform contentContainer, ScrollRect scrollRect, RectTransform scrollviewContainer, HeaderNumberDisplay headerNumberDisplay, GridModel gridModel)
        {
            _contentContainer = contentContainer;
            _scrollRect = scrollRect;
            _scrollviewContainer = scrollviewContainer;
            _headerNumberDisplay = headerNumberDisplay;
            _gridModel = gridModel;
            
            _topPaddingValue = GameConstants.CellSize * 1.1f;
            _scrollLoggingThreshold = GameConstants.CellSize / 2f;
        }

        public void Initialize()
        {
            if (!_scrollRect) return;
            _lastLoggedScrollPosition = _scrollRect.content.anchoredPosition.y;
        }

        public void Dispose()
        {
            UnsubscribeFromScrollEvents();
        }

        public void SubscribeToScrollEvents()
        {
            _scrollRect?.onValueChanged.AddListener(OnScrollValueChanged);
        }

        public void UnsubscribeFromScrollEvents()
        {
            _scrollRect?.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        /// <summary>
        /// Обновляет высоту контейнера сетки в зависимости от количества линий.
        /// </summary>
        public void UpdateContentSize()
        {
            if (!_contentContainer) return;
            var lineCount = _gridModel.Cells.Count;
            var newHeight = lineCount * GameConstants.CellSize + GameConstants.Indent;
            _contentContainer.sizeDelta = new Vector2(_contentContainer.sizeDelta.x, newHeight);
        }

        /// <summary>
        /// Обновляет позицию визуального представления ячейки.
        /// </summary>
        /// <param name="data">Данные ячейки.</param>
        /// <param name="cellView">Визуальное представление ячейки.</param>
        /// <param name="animate">Нужно ли анимировать перемещение.</param>
        public void UpdateCellPosition(CellData data, Cell cellView, bool animate)
        {
            var targetPosition = GetCellPosition(data);

            if (!cellView.Animator) return;
            var duration = animate ? GameConstants.CellMoveDuration : 0f;
            cellView.Animator.MoveTo(cellView.TargetRectTransform, targetPosition, duration);
        }

        /// <summary>
        /// Обновляет верхнюю строку с номерами при прокрутке.
        /// </summary>
        public void RefreshTopLine()
        {
            if (!_headerNumberDisplay) return;
            var numberLine = Mathf.RoundToInt(_lastLoggedScrollPosition / GameConstants.CellSize);
            var activeNumbers = _gridModel.GetNumbersForTopLine(numberLine);
            _headerNumberDisplay.UpdateDisplayedNumbers(activeNumbers);
        }

        /// <summary>
        /// Включает или отключает отступ сверху для контейнера прокрутки.
        /// </summary>
        public void SetTopPaddingActive(bool isActive)
        {
            if (_scrollviewContainer)
            {
                var topOffset = isActive ? -_topPaddingValue : 0;
                _scrollviewContainer.offsetMax = new Vector2(_scrollviewContainer.offsetMax.x, topOffset);
            }
        }

        private void OnScrollValueChanged(Vector2 position)
        {
            var currentScrollPosition = _scrollRect.content.anchoredPosition.y;
            if (!(Mathf.Abs(currentScrollPosition - _lastLoggedScrollPosition) >= _scrollLoggingThreshold)) return;
            _lastLoggedScrollPosition = currentScrollPosition;
            RefreshTopLine();
        }

        private Vector2 GetCellPosition(CellData data)
        {
            return new Vector2(
                GameConstants.CellSize * data.Column + GameConstants.Indent / 2f,
                -GameConstants.CellSize * data.Line - GameConstants.Indent / 2f
            );
        }
    }
}