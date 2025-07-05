using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class FloatingScore : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private float fadeOutTime = 0.5f;
        [SerializeField] private float lifeTime = 1f;

        private RectTransform _rectTransform;
        private Action<FloatingScore> _onComplete;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Show(string text, Color color, Vector2 centerPosition, Vector2 size, Action<FloatingScore> onComplete)
        {
            _onComplete = onComplete;
            scoreText.text = text;
            scoreText.color = color;
            _rectTransform.sizeDelta = size;
            _rectTransform.anchoredPosition = centerPosition;

            gameObject.SetActive(true);
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            var elapsedTime = 0f;
            var startColor = scoreText.color;

            while (elapsedTime < lifeTime)
            {
                if (elapsedTime > lifeTime - fadeOutTime)
                {
                    var alpha = Mathf.Lerp(1f, 0f, (elapsedTime - (lifeTime - fadeOutTime)) / fadeOutTime);
                    scoreText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            scoreText.color = startColor;
            _onComplete?.Invoke(this);
        }
    }
}