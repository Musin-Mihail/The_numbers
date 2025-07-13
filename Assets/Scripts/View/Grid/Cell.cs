using System;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.Grid
{
    /// <summary>
    /// Визуальное представление одной ячейки на игровой сетке.
    /// Отвечает за отображение числа, обработку кликов и визуальные состояния (выделение, подсветка).
    /// </summary>
    [RequireComponent(typeof(CellAnimator), typeof(Image))]
    public class Cell : MonoBehaviour
    {
        public TextMeshProUGUI text;
        [SerializeField] private Color originalColor;
        [SerializeField] private Color hintColor;
        [SerializeField] private Color selectColor;
        [SerializeField] private RectTransform targetRectTransform;
        [SerializeField] private CellAnimator animator;
        [SerializeField] private Image backgroundImage;
        
        /// <summary>
        /// RectTransform, который используется для позиционирования.
        /// </summary>
        public RectTransform TargetRectTransform => targetRectTransform;
        
        /// <summary>
        /// Компонент для анимации ячейки.
        /// </summary>
        public CellAnimator Animator => animator;

        /// <summary>
        /// Callback, вызываемый при клике на ячейку.
        /// </summary>
        public Action<Guid> OnClickedCallback { get; set; }
        private Guid DataId { get; set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        private bool IsActive { get; set; }
        public int Number { get; private set; }

        private bool _selected;
        private bool _isHighlighted;

        /// <summary>
        /// Обновляет состояние ячейки на основе данных из модели.
        /// </summary>
        /// <param name="data">Данные ячейки.</param>
        public void UpdateFromData(CellData data)
        {
            DataId = data.Id;
            Line = data.Line;
            Column = data.Column;
            Number = data.Number;
            text.text = Number.ToString();
            IsActive = data.IsActive;
            SetVisualState(IsActive);
        }

        /// <summary>
        /// Обрабатывает клик по ячейке.
        /// </summary>
        public void HandleClick()
        {
            if (!IsActive) return;
            OnClickedCallback?.Invoke(DataId);
        }

        /// <summary>
        /// Сбрасывает состояние ячейки перед возвращением в пул объектов.
        /// </summary>
        public void ResetForPooling()
        {
            OnClickedCallback = null;
            _isHighlighted = false;
            SetSelected(false);
        }

        /// <summary>
        /// Устанавливает состояние выделения ячейки.
        /// </summary>
        /// <param name="isSelected">True, если ячейка выбрана.</param>
        public void SetSelected(bool isSelected)
        {
            if (_selected == isSelected) return;
            _selected = isSelected;

            if (!backgroundImage) return;

            if (_selected)
            {
                backgroundImage.color = selectColor;
            }
            else
            {
                backgroundImage.color = _isHighlighted ? hintColor : originalColor;
            }
        }

        /// <summary>
        /// Устанавливает состояние подсветки (для подсказок).
        /// </summary>
        /// <param name="show">True, если нужно подсветить.</param>
        public void SetHighlight(bool show)
        {
            _isHighlighted = show;

            if (!_selected && backgroundImage)
            {
                backgroundImage.color = _isHighlighted ? hintColor : originalColor;
            }
        }

        /// <summary>
        /// Устанавливает визуальное состояние (активна/неактивна).
        /// </summary>
        /// <param name="isActive">Активность ячейки.</param>
        public void SetVisualState(bool isActive)
        {
            text.enabled = isActive;
            if (isActive) return;
            SetHighlight(false);
            SetSelected(false);
        }
    }
}