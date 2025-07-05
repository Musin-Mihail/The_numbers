using System;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.Grid
{
    [RequireComponent(typeof(CellAnimator), typeof(Image))]
    public class Cell : MonoBehaviour
    {
        public TextMeshProUGUI text;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Color originalColor;
        [SerializeField] private Color hintColor;
        [SerializeField] private Color selectColor;
        [SerializeField] private RectTransform targetRectTransform;
        [SerializeField] private CellAnimator animator;
        [SerializeField] private Image backgroundImage;
        public RectTransform TargetRectTransform => targetRectTransform;
        public CellAnimator Animator => animator;

        public Action<Guid> OnClickedCallback { get; set; }
        private Guid DataId { get; set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        private bool IsActive { get; set; }
        public int Number { get; private set; }
        private bool _selected;

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

        public void HandleClick()
        {
            if (!IsActive) return;
            OnClickedCallback?.Invoke(DataId);
        }

        public void ResetForPooling()
        {
            OnClickedCallback = null;
            SetSelected(false);
            SetHighlight(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (_selected == isSelected) return;
            _selected = isSelected;
            backgroundImage.color = _selected ? selectColor : originalColor;
        }

        public void SetHighlight(bool show)
        {
            if (backgroundImage)
            {
                backgroundImage.color = show ? hintColor : originalColor;
            }
        }

        public void SetVisualState(bool isActive)
        {
            text.enabled = isActive;
            if (!isActive)
            {
                SetHighlight(false);
            }
        }
    }
}