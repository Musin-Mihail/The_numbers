using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Localization
{
    /// <summary>
    /// Компонент, который автоматически обновляет текстовое поле при смене языка.
    /// Вешается на каждый GameObject с Text или TextMeshProUGUI.
    /// Использует систему Font Fallbacks из TextMesh Pro для поддержки разных языков.
    /// </summary>
    public class LocalizableText : MonoBehaviour
    {
        [Tooltip("Ключ для поиска перевода в JSON файле.")]
        [SerializeField] private string localizationKey;

        private TextMeshProUGUI _textMeshPro;
        private Text _text;
        private LocalizationManager _localizationManager;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
            if (!_textMeshPro)
            {
                _text = GetComponent<Text>();
            }
        }

        private void OnEnable()
        {
            _localizationManager ??= ServiceProvider.GetService<LocalizationManager>();
            if (_localizationManager == null)
            {
                Debug.LogWarning($"[LocalizableText] LocalizationManager не найден для ключа '{localizationKey}'. Компонент будет отключен.", this);
                enabled = false;
                return;
            }

            LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            if (_localizationManager != null)
            {
                LocalizationManager.OnLanguageChanged -= UpdateText;
            }
        }

        /// <summary>
        /// Получает перевод и обновляет текст.
        /// Смена шрифта больше не требуется, TextMesh Pro обработает это через Fallbacks.
        /// </summary>
        private void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey) || _localizationManager == null) return;

            var translation = _localizationManager.Get(localizationKey);

            if (_textMeshPro)
            {
                _textMeshPro.text = translation;
            }
            else if (_text)
            {
                _text.text = translation;
            }
        }
    }
}