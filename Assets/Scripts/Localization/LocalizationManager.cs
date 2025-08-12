using System;
using System.Collections.Generic;
using Core.Events;
using Newtonsoft.Json;
using UnityEngine;

namespace Localization
{
    /// <summary>
    /// Загружает языковые файлы и предоставляет доступ к переводам.
    /// Работает с единым JSON файлом для всех языков.
    /// </summary>
    public class LocalizationManager
    {
        private Dictionary<string, Dictionary<string, string>> _allTranslations;

        /// <summary>
        /// Возвращает код текущего установленного языка.
        /// </summary>
        public string CurrentLanguage { get; private set; }

        public static event Action OnLanguageChanged;

        public LocalizationManager(GameEvents gameEvents)
        {
            gameEvents.onSetLanguage.AddListener(SetLanguage);
            LoadTranslationsFile();
        }

        /// <summary>
        /// Устанавливает начальный язык.
        /// </summary>
        public void SetInitialLanguage(string langCode)
        {
            SetLanguage(langCode);
        }

        /// <summary>
        /// Загружает единый файл локализации из папки Resources/Localization.
        /// </summary>
        private void LoadTranslationsFile()
        {
            var asset = Resources.Load<TextAsset>("Localization/translations");
            if (asset != null)
            {
                Debug.Log(asset.text);
                _allTranslations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(asset.text);
                Debug.Log("[LocalizationManager] Файл переводов успешно загружен.");
            }
            else
            {
                Debug.LogError("[LocalizationManager] Файл 'translations.json' не найден в Resources/Localization.");
                _allTranslations = new Dictionary<string, Dictionary<string, string>>();
            }
        }

        /// <summary>
        /// Устанавливает текущий язык и уведомляет подписчиков.
        /// </summary>
        private void SetLanguage(string langCode)
        {
            CurrentLanguage = string.IsNullOrEmpty(langCode) ? "en" : langCode;
            Debug.Log($"[LocalizationManager] Язык изменен на '{CurrentLanguage}'.");
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Возвращает перевод по ключу для текущего языка.
        /// </summary>
        public string Get(string key)
        {
            if (_allTranslations.TryGetValue(key, out var translationsForCurrentKey))
            {
                if (translationsForCurrentKey.TryGetValue(CurrentLanguage, out var translation))
                {
                    return translation;
                }

                if (translationsForCurrentKey.TryGetValue("en", out var fallbackTranslation))
                {
                    Debug.LogWarning($"[LocalizationManager] Перевод для ключа '{key}' на языке '{CurrentLanguage}' не найден. Используется 'en'.");
                    return fallbackTranslation;
                }
            }

            Debug.LogError($"[LocalizationManager] Перевод для ключа '{key}' не найден ни для одного языка.");
            return $"#{key}#";
        }
    }
}