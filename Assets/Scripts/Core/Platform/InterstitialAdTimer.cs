using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Управляет таймером для показа межстраничной рекламы и соответствующим UI.
    /// </summary>
    public class InterstitialAdTimer : MonoBehaviour
    {
        [Header("Кнопки управления таймером")]
        [Tooltip("Кнопка для отключения таймера (значение 0)")]
        [SerializeField] private Button noTimerButton;
        [Tooltip("Кнопка для таймера на 70 секунд")]
        [SerializeField] private Button timer70Button;
        [Tooltip("Кнопка для таймера на 140 секунд")]
        [SerializeField] private Button timer140Button;
        [Tooltip("Кнопка для таймера на 280 секунд")]
        [SerializeField] private Button timer280Button;
        [Header("Визуальные настройки кнопок")]
        [Tooltip("Цвет для выделения активной кнопки")]
        [SerializeField] private Color selectedColor = new(0.4f, 1.0f, 0.4f); // Ярко-зеленый
        [Tooltip("Стандартный цвет неактивных кнопок")]
        [SerializeField] private Color defaultColor = Color.white;
        private Dictionary<int, Button> _timerButtons;
        private Coroutine _timerCoroutine;
        private int _selectedDuration;

        private void OnEnable()
        {
            YG2.onGetSDKData += Initialize;
            if (YG2.isSDKEnabled)
            {
                Initialize();
            }
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= Initialize;
        }

        /// <summary>
        /// Инициализация компонента: настройка кнопок и загрузка состояния.
        /// </summary>
        private void Initialize()
        {
            _timerButtons = new Dictionary<int, Button>
            {
                { 0, noTimerButton },
                { 70, timer70Button },
                { 140, timer140Button },
                { 280, timer280Button }
            };
            noTimerButton.onClick.AddListener(() => SetTimer(0));
            timer70Button.onClick.AddListener(() => SetTimer(70));
            timer140Button.onClick.AddListener(() => SetTimer(140));
            timer280Button.onClick.AddListener(() => SetTimer(280));
            LoadState();
        }

        /// <summary>
        /// Загружает сохраненное значение таймера и применяет его.
        /// </summary>
        private void LoadState()
        {
            _selectedDuration = YG2.saves.interstitialAdTimerValue;
            Debug.Log($"InterstitialAdTimer: Загружено значение таймера: {_selectedDuration}");
            UpdateVisuals();
            StartTimer();
        }

        /// <summary>
        /// Устанавливает новое значение таймера, сохраняет его и перезапускает таймер.
        /// </summary>
        /// <param name="duration">Длительность в секундах (0 для отключения).</param>
        private void SetTimer(int duration)
        {
            if (_selectedDuration == duration) return;
            Debug.Log($"InterstitialAdTimer: Установка таймера на {duration} секунд.");
            _selectedDuration = duration;
            YG2.saves.interstitialAdTimerValue = _selectedDuration;
            YG2.SaveProgress();
            UpdateVisuals();
            StartTimer();
        }

        /// <summary>
        /// Запускает или останавливает корутину таймера в зависимости от выбранной длительности.
        /// </summary>
        private void StartTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            if (_selectedDuration > 0)
            {
                _timerCoroutine = StartCoroutine(TimerCoroutine(_selectedDuration));
            }
            else
            {
                Debug.Log("InterstitialAdTimer: Таймер отключен.");
            }
        }

        /// <summary>
        /// Корутина, которая отсчитывает время и вызывает показ рекламы.
        /// </summary>
        /// <param name="duration">Время ожидания в секундах.</param>
        private IEnumerator TimerCoroutine(float duration)
        {
            Debug.Log($"InterstitialAdTimer: Таймер запущен на {duration} секунд.");
            yield return new WaitForSeconds(duration);
            Debug.Log("InterstitialAdTimer: Таймер завершен. Показ межстраничной рекламы.");
            YG2.InterstitialAdvShow();
            StartTimer();
        }

        /// <summary>
        /// Обновляет внешний вид кнопок, подсвечивая выбранную.
        /// </summary>
        private void UpdateVisuals()
        {
            foreach (var pair in _timerButtons)
            {
                if (!pair.Value) continue;
                var image = pair.Value.GetComponent<Image>();
                if (image)
                {
                    image.color = (pair.Key == _selectedDuration) ? selectedColor : defaultColor;
                }
            }
        }
    }
}