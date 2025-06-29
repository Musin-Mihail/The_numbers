using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Button))]
    public class ButtonAnimator : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = Color.green;
        [SerializeField] private float animationSpeed = 2.0f;
        [SerializeField] private float scaleAmount = 1.1f;
        [SerializeField] private float rotationAmount = 5f;

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

            GameEvents.OnAddExistingNumbers += StopAnimation;
            GameEvents.OnNoHintFound += StartAnimation;
        }

        private void OnDestroy()
        {
            GameEvents.OnAddExistingNumbers -= StopAnimation;
            GameEvents.OnNoHintFound -= StartAnimation;
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
        }

        private void StartAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(AnimateButton());
        }

        private void StopAnimation()
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