﻿using System.Collections;
using Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    /// <summary>
    /// Анимирует кнопку (масштаб, вращение, цвет) для привлечения внимания.
    /// Управляется через игровые события.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(Button))]
    public class ButtonAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Color highlightColor = Color.green;
        [SerializeField] private float animationSpeed = 2.0f;
        [SerializeField] private float scaleAmount = 1.1f;
        [SerializeField] private float rotationAmount = 5f;

        [Header("Event Listening")]
        [SerializeField] private VoidEvent onAnimationStart;
        [SerializeField] private VoidEvent onAnimationStop;

        private RectTransform _rectTransform;
        private Image _buttonImage;
        private Color _originalColor;
        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private Coroutine _animationCoroutine;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _buttonImage = GetComponent<Image>();

            if (_buttonImage)
            {
                _originalColor = _buttonImage.color;
            }

            _originalScale = _rectTransform.localScale;
            _originalRotation = _rectTransform.localRotation;
        }

        private void OnEnable()
        {
            if (onAnimationStart) onAnimationStart.AddListener(StartAnimation);
            if (onAnimationStop) onAnimationStop.AddListener(StopAnimation);
        }

        private void OnDisable()
        {
            if (onAnimationStart) onAnimationStart.RemoveListener(StartAnimation);
            if (onAnimationStop) onAnimationStop.RemoveListener(StopAnimation);

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
        }

        /// <summary>
        /// Запускает корутину анимации.
        /// </summary>
        public void StartAnimation()
        {
            if (!enabled || !gameObject.activeInHierarchy) return;
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(AnimateButton());
        }

        /// <summary>
        /// Останавливает анимацию и возвращает кнопку в исходное состояние.
        /// </summary>
        public void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            if (_buttonImage)
            {
                _buttonImage.color = _originalColor;
            }

            _rectTransform.localScale = _originalScale;
            _rectTransform.localRotation = _originalRotation;
        }

        private IEnumerator AnimateButton()
        {
            if (_buttonImage)
            {
                _buttonImage.color = highlightColor;
            }

            while (true)
            {
                var sineWaveForRotation = Mathf.Sin(Time.time * animationSpeed * 2);
                var sineWaveForScale = (Mathf.Sin(Time.time * animationSpeed) + 1f) / 2f;
                _rectTransform.localScale = _originalScale * Mathf.Lerp(1f, scaleAmount, sineWaveForScale);
                _rectTransform.localRotation = _originalRotation * Quaternion.Euler(0, 0, sineWaveForRotation * rotationAmount);
                yield return null;
            }
        }
    }
}