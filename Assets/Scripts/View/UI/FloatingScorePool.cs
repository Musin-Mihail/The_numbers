using System.Collections.Generic;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Пул объектов для всплывающих текстов (FloatingScore).
    /// </summary>
    public class FloatingScorePool : MonoBehaviour
    {
        [SerializeField] private GameObject floatingScorePrefab;
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private GameObject parent;

        private readonly Queue<FloatingScore> _pool = new();

        private void Start()
        {
            for (var i = 0; i < initialPoolSize; i++)
            {
                var scoreInstance = CreateNewInstance();
                ReturnScore(scoreInstance);
            }
        }

        private FloatingScore CreateNewInstance()
        {
            var go = Instantiate(floatingScorePrefab, canvasTransform);
            go.transform.SetParent(parent.transform);
            var floatingScore = go.GetComponent<FloatingScore>();
            return floatingScore;
        }

        /// <summary>
        /// Получает экземпляр FloatingScore из пула.
        /// </summary>
        public FloatingScore GetScore()
        {
            if (_pool.Count <= 0) return CreateNewInstance();
            var scoreInstance = _pool.Dequeue();
            scoreInstance.gameObject.SetActive(true);
            return scoreInstance;
        }

        /// <summary>
        /// Возвращает экземпляр FloatingScore в пул.
        /// </summary>
        public void ReturnScore(FloatingScore scoreInstance)
        {
            scoreInstance.gameObject.SetActive(false);
            _pool.Enqueue(scoreInstance);
        }
    }
}