using System.Collections;
using System.Collections.Generic;
using Core.Events;
using TMPro;
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
        [SerializeField] private Color selectedColor = new(0.4f, 1.0f, 0.4f);
        [Tooltip("Стандартный цвет неактивных кнопок")]
        [SerializeField] private Color defaultColor = new(0.53f, 0.70f, 0.85f);

        [Header("Обратный отсчет")]
        [Tooltip("Панель с обратным отсчетом")]
        [SerializeField] private GameObject countdownPanel;
        [Tooltip("Текстовое поле для обратного отсчета перед рекламой")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Каналы событий")]
        [Tooltip("Ссылка на ScriptableObject с игровыми событиями")]
        [SerializeField] private GameEvents gameEvents;

        private Dictionary<int, Button> _timerButtons;
        private Coroutine _timerCoroutine;
        private int _selectedDuration;
        private bool _isGameActive;

        private void Awake()
        {
            if (countdownPanel)
            {
                countdownPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            YG2.onGetSDKData += Initialize;
            if (gameEvents)
            {
                gameEvents.onShowMenu.AddListener(HandleShowMenu);
                gameEvents.onHideMenu.AddListener(HandleHideMenu);
            }

            if (YG2.isSDKEnabled)
            {
                Initialize();
            }
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= Initialize;
            if (!gameEvents) return;
            gameEvents.onShowMenu.RemoveListener(HandleShowMenu);
            gameEvents.onHideMenu.RemoveListener(HandleHideMenu);
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
        /// Обработчик события показа игрового меню.
        /// </summary>
        private void HandleShowMenu()
        {
            _isGameActive = false;
            Debug.Log("InterstitialAdTimer: Игровое поле закрыто, останавливаем таймер.");
            StopCurrentTimer();
        }

        /// <summary>
        /// Обработчик события скрытия игрового меню.
        /// </summary>
        private void HandleHideMenu()
        {
            _isGameActive = true;
            Debug.Log("InterstitialAdTimer: Игровое поле открыто, запускаем таймер.");
            StartTimer();
        }

        /// <summary>
        /// Останавливает текущую корутину таймера.
        /// </summary>
        private void StopCurrentTimer()
        {
            if (_timerCoroutine == null) return;
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        /// <summary>
        /// Загружает сохраненное значение таймера и применяет его.
        /// </summary>
        private void LoadState()
        {
            _selectedDuration = YG2.saves.interstitialAdTimerValue;
            Debug.Log($"InterstitialAdTimer: Загружено значение таймера: {_selectedDuration}");
            UpdateVisuals();
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
            StopCurrentTimer();
            switch (_isGameActive)
            {
                case true when _selectedDuration > 0:
                    _timerCoroutine = StartCoroutine(TimerCoroutine(_selectedDuration));
                    break;
                case false:
                    Debug.Log("InterstitialAdTimer: Таймер не запущен, так как игровое поле неактивно.");
                    break;
                default:
                    Debug.Log("InterstitialAdTimer: Таймер отключен (длительность 0).");
                    break;
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
            StartTimer();
        }

        /// <summary>
        /// Обновляет внешний вид кнопок, подсвечивая выбранную.
        /// </summary>
        private void UpdateVisuals()
        {
            Debug.Log(defaultColor.ToString());
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