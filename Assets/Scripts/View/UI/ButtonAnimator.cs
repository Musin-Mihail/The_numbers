using System.Collections;
using Core;
using Core.Events;
using UnityEngine;
using UnityEngine.UI;
using YG;

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
        [Tooltip("Событие, которое запускает анимацию.")]
        [SerializeField] private VoidEvent onAnimationStart;
        [Tooltip("Событие, которое останавливает анимацию.")]
        [SerializeField] private VoidEvent onAnimationStop;
        [Header("Behavior")]
        [Tooltip("Отметьте, если эта кнопка используется специально для уведомлений об обновлении.")]
        [SerializeField] private bool isForUpdateNotification;
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
            if (isForUpdateNotification && YG2.isSDKEnabled && YG2.saves.seenUpdateVersion < GameConstants.GameVersion)
            {
                StartAnimation();
            }
        }

        private void OnDisable()
        {
            if (onAnimationStart) onAnimationStart.RemoveListener(StartAnimation);
            if (onAnimationStop) onAnimationStop.RemoveListener(StopAnimation);

            StopAnimation();
        }

        /// <summary>
        /// Запускает корутину анимации.
        /// </summary>
        private void StartAnimation()
        {
            if (_animationCoroutine != null) return;
            if (!enabled || !gameObject.activeInHierarchy) return;
            _animationCoroutine = StartCoroutine(AnimateButton());
        }

        /// <summary>
        /// Останавливает анимацию и возвращает кнопку в исходное состояние.
        /// </summary>
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

            if (!_rectTransform) return;
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