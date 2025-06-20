using System.Collections;
using UnityEngine;

namespace View.Grid
{
    public class CellAnimator : MonoBehaviour
    {
        private Coroutine _moveCoroutine;

        public void MoveTo(RectTransform rectTransform, Vector2 targetPosition, float duration)
        {
            if (!rectTransform)
            {
                Debug.LogWarning("RectTransform is null, cannot start animation.", this);
                return;
            }

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(AnimateMoveCoroutine(rectTransform, targetPosition, duration));
        }

        private IEnumerator AnimateMoveCoroutine(RectTransform rectTransform, Vector2 targetPosition, float duration)
        {
            var startPosition = rectTransform.anchoredPosition;
            if (Vector2.Distance(startPosition, targetPosition) < 0.01f)
            {
                rectTransform.anchoredPosition = targetPosition;
                _moveCoroutine = null;
                yield break;
            }

            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                var t = elapsedTime / duration;
                t = t * t * (3f - 2f * t);

                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = targetPosition;
            _moveCoroutine = null;
        }
    }
}