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
        [SerializeField] private GameObject indicator;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite disabledSprite;
        [SerializeField] private Color hintColor = new(0.6f, 1f, 0.6f, 1f);

        private RectTransform _targetRectTransform;

        public RectTransform TargetRectTransform
        {
            get
            {
                if (!_targetRectTransform)
                {
                    _targetRectTransform = GetComponent<RectTransform>();
                }

                return _targetRectTransform;
            }
        }

        private CellAnimator _animator;

        public CellAnimator Animator
        {
            get
            {
                if (!_animator)
                {
                    _animator = GetComponent<CellAnimator>();
                }

                return _animator;
            }
        }

        private Image _backgroundImage;

        private Image BackgroundImage
        {
            get
            {
                if (!_backgroundImage)
                {
                    _backgroundImage = GetComponent<Image>();
                }

                return _backgroundImage;
            }
        }

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
            indicator.SetActive(_selected);
        }

        public void SetHighlight(bool show)
        {
            if (BackgroundImage)
            {
                BackgroundImage.color = show ? hintColor : Color.white;
            }
        }

        public void SetVisualState(bool isActive)
        {
            text.enabled = isActive;
            if (!BackgroundImage) return;
            BackgroundImage.sprite = isActive ? activeSprite : disabledSprite;
            if (!isActive)
            {
                SetHighlight(false);
            }
        }
    }
}