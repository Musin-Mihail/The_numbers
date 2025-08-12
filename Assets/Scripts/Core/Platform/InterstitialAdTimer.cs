using System.Collections;
using Core.Events;
using TMPro;
using UnityEngine;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Управляет показом межстраничной рекламы по событию, соблюдая временной кулдаун.
    /// </summary>
    public class InterstitialAdTimer : MonoBehaviour
    {
        [Header("UI Обратного отсчета")]
        [Tooltip("Панель с обратным отсчетом")]
        [SerializeField] private GameObject countdownPanel;
        [Tooltip("Текстовое поле для обратного отсчета перед рекламой")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Каналы событий")]
        [Tooltip("Ссылка на ScriptableObject с игровыми событиями")]
        [SerializeField] private GameEvents gameEvents;

        private const float AdCooldown = 70.0f;
        private float _lastAdShowTime;

        private void Awake()
        {
            if (countdownPanel)
            {
                countdownPanel.SetActive(false);
            }

            _lastAdShowTime = Time.time;
        }

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onAddExistingNumbers.AddListener(OnAddExistingNumbersTriggered);
            }
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onAddExistingNumbers.RemoveListener(OnAddExistingNumbersTriggered);
            }
        }

        /// <summary>
        /// Вызывается при срабатывании события onAddExistingNumbers.
        /// Проверяет, прошел ли кулдаун для показа рекламы.
        /// </summary>
        private void OnAddExistingNumbersTriggered()
        {
            if (Time.time >= _lastAdShowTime + AdCooldown)
            {
                StartCoroutine(ShowAdWithCountdownCoroutine());
            }
            else
            {
                var timeLeft = Mathf.RoundToInt((_lastAdShowTime + AdCooldown) - Time.time);
                Debug.Log($"InterstitialAdTimer: Реклама не показана. Осталось до конца кулдауна: {timeLeft}с.");
            }
        }

        /// <summary>
        /// Корутина, которая отображает обратный отсчет и затем показывает межстраничную рекламу.
        /// </summary>
        private IEnumerator ShowAdWithCountdownCoroutine()
        {
            Debug.Log("InterstitialAdTimer: Запуск обратного отсчета перед рекламой.");
            if (countdownPanel) countdownPanel.SetActive(true);
            for (var i = 3; i > 0; i--)
            {
                if (countdownText)
                {
                    countdownText.text = $"Реклама через: {i}с";
                }

                yield return new WaitForSeconds(1.0f);
            }

            if (countdownPanel) countdownPanel.SetActive(false);
            YG2.InterstitialAdvShow();
            _lastAdShowTime = Time.time;
            Debug.Log("InterstitialAdTimer: Реклама показана, кулдаун сброшен.");
        }
    }
}