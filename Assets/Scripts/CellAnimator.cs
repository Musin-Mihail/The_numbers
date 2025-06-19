using System.Collections;
using UnityEngine;

public class CellAnimator : MonoBehaviour
{
    private Coroutine _moveCoroutine;

    public void MoveTo(RectTransform rectTransform, Vector2 targetPosition, float duration)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        _moveCoroutine = StartCoroutine(AnimateMoveCoroutine(rectTransform, targetPosition, duration));
    }

    private IEnumerator AnimateMoveCoroutine(RectTransform rectTransform, Vector2 targetPosition, float duration)
    {
        if (!rectTransform) yield break;
        var startPosition = rectTransform.anchoredPosition;
        var elapsedTime = 0f;
        if (Vector2.Distance(startPosition, targetPosition) < 0.01f)
        {
            rectTransform.anchoredPosition = targetPosition;
            yield break;
        }

        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        _moveCoroutine = null;
    }
}