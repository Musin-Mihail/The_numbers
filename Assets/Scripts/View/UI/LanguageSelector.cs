using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Events;
using Localization;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    /// <summary>
    /// Управляет UI для выбора языка в игре.
    /// </summary>
    public class LanguageSelector : MonoBehaviour
    {
        /// <summary>
        /// Структура для сопоставления кода языка и его иконки в инспекторе.
        /// </summary>
        [Serializable]
        public struct LanguageSpriteMapping
        {
            public string languageCode;
            public Sprite languageSprite;
        }

        [Header("UI Элементы")]
        [Tooltip("Image компонент для отображения иконки текущего языка.")]
        [SerializeField] private Image currentLanguageImage;
        [Tooltip("Кнопка, открывающая панель выбора языка.")]
        [SerializeField] private Button openLanguagePanelButton;

        [Tooltip("Панель с выбором языков.")]
        [SerializeField] private GameObject languagePanel;

        [Tooltip("Кнопка для закрытия панели выбора языка.")]
        [SerializeField] private Button closeButton;

        [Tooltip("Фоновая кнопка для закрытия панели при клике вне окна.")]
        [SerializeField] private Button backgroundCloseButton;

        [Header("Настройки языков")]
        [Tooltip("Список сопоставлений кодов языков (например, 'ru', 'en') и их иконок.")]
        [SerializeField] private List<LanguageSpriteMapping> languageMappings;

        private GameEvents _gameEvents;
        private LocalizationManager _localizationManager;
        private readonly Dictionary<string, Sprite> _spriteMap = new();

        private void Awake()
        {
            foreach (var mapping in languageMappings.Where(mapping => !_spriteMap.ContainsKey(mapping.languageCode)))
            {
                _spriteMap.Add(mapping.languageCode, mapping.languageSprite);
            }
        }

        private void Start()
        {
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            _localizationManager = ServiceProvider.GetService<LocalizationManager>();

            if (!_gameEvents || _localizationManager == null)
            {
                Debug.LogError("LanguageSelector не смог получить GameEvents или LocalizationManager. Компонент будет отключен.", this);
                enabled = false;
                return;
            }

            openLanguagePanelButton.onClick.AddListener(ShowPanel);
            closeButton.onClick.AddListener(HidePanel);
            backgroundCloseButton.onClick.AddListener(HidePanel);
            languagePanel.SetActive(false);
            UpdateCurrentLanguageIcon();
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += UpdateCurrentLanguageIcon;
            UpdateCurrentLanguageIcon();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= UpdateCurrentLanguageIcon;
        }

        /// <summary>
        /// Показывает панель выбора языка.
        /// </summary>
        private void ShowPanel()
        {
            languagePanel.SetActive(true);
        }

        /// <summary>
        /// Скрывает панель выбора языка.
        /// </summary>
        private void HidePanel()
        {
            languagePanel.SetActive(false);
        }

        /// <summary>
        /// Вызывается при нажатии на кнопку выбора языка. Этот метод нужно вызывать из инспектора Unity.
        /// </summary>
        /// <param name="languageCode">Код выбранного языка (e.g., "ru", "en").</param>
        public void SelectLanguage(string languageCode)
        {
            _gameEvents.onSetLanguage.Raise(languageCode);
            HidePanel();
        }

        /// <summary>
        /// Обновляет иконку в углу экрана в соответствии с текущим языком.
        /// </summary>
        private void UpdateCurrentLanguageIcon()
        {
            if (_localizationManager == null || currentLanguageImage == null) return;

            var currentLang = _localizationManager.CurrentLanguage;
            if (_spriteMap.TryGetValue(currentLang, out var sprite))
            {
                currentLanguageImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"Спрайт для языка '{currentLang}' не найден.", this);
            }
        }

        private void OnDestroy()
        {
            if (openLanguagePanelButton) openLanguagePanelButton.onClick.RemoveAllListeners();
            if (closeButton) closeButton.onClick.RemoveAllListeners();
            if (backgroundCloseButton) backgroundCloseButton.onClick.RemoveAllListeners();
        }
    }
}