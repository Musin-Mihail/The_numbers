using Core.Events;
using UnityEngine;
using YG;

namespace Localization
{
    /// <summary>
    /// Компонент-мост для отслеживания смены языка из Debug-панели Яндекс Игр.
    /// Он подписывается на событие плагина YG2.onSwitchLang и вызывает
    /// соответствующее событие в вашей игровой системе (GameEvents).
    /// </summary>
    public class YandexLanguageListener : MonoBehaviour
    {
        [Header("Каналы событий")]
        [Tooltip("Ссылка на ScriptableObject с игровыми событиями вашего проекта")]
        [SerializeField] private GameEvents gameEvents;

        /// <summary>
        /// Подписываемся на событие смены языка плагина при активации объекта.
        /// </summary>
        private void OnEnable()
        {
            if (!gameEvents)
            {
                Debug.LogError("ОШИБКА: 'GameEvents' не назначен в инспекторе для YandexLanguageListener!", this);
                return;
            }

            YG2.onSwitchLang += HandleLanguageChange;
            Debug.Log("YandexLanguageListener подписался на событие смены языка.");
        }

        /// <summary>
        /// Отписываемся от события при деактивации объекта, чтобы избежать утечек памяти.
        /// </summary>
        private void OnDisable()
        {
            YG2.onSwitchLang -= HandleLanguageChange;
            Debug.Log("YandexLanguageListener отписался от события смены языка.");
        }

        /// <summary>
        /// Этот метод будет вызван плагином YG2 при смене языка.
        /// </summary>
        /// <param name="langCode">Новый код языка (например, "en", "ru", "tr").</param>
        private void HandleLanguageChange(string langCode)
        {
            Debug.Log($"[YandexLanguageListener] Получен новый язык от плагина: {langCode}");
            if (YG2.saves.language != langCode)
            {
                gameEvents.onSetLanguage.Raise(langCode);
            }
            else
            {
                Debug.Log($"[YandexLanguageListener] Смена языка проигнорирована, так как язык '{langCode}' уже установлен.");
            }
        }
    }
}