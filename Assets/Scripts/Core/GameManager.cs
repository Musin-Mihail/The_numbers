using System.Collections;
using Core.Events;
using Interfaces;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Управляет глобальным состоянием игры, таким как сохранение прогресса,
    /// обработка паузы и выхода из приложения.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private GameEvents _gameEvents;
        private ISaveLoadService _saveLoadService;

        private const float SaveCooldown = 5.0f;
        private float _timeSinceLastSave = 5.0f;
        private bool _savePending;
        private bool _isSaving;

        /// <summary>
        /// Инициализация зависимостей, полученных из GameBootstrap.
        /// </summary>
        public void Initialize(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            SubscribeToEvents();
        }

        /// <summary>
        /// Проверяет, нужно ли выполнить отложенное сохранение.
        /// </summary>
        private void Update()
        {
            if (_timeSinceLastSave < SaveCooldown)
            {
                _timeSinceLastSave += Time.deltaTime;
            }

            if (_savePending && _timeSinceLastSave >= SaveCooldown && !_isSaving)
            {
                StartCoroutine(SaveGameCoroutine());
            }
        }

        /// <summary>
        /// Отписка от событий при уничтожении объекта.
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Подписывается на игровые события.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.AddListener(SetTopLineVisibilityAndSave);
        }

        /// <summary>
        /// Отписывается от игровых событий.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.RemoveListener(SetTopLineVisibilityAndSave);
        }

        /// <summary>
        /// Устанавливает видимость верхней строки и сохраняет это состояние.
        /// </summary>
        /// <param name="isVisible">Видимость строки.</param>
        private void SetTopLineVisibilityAndSave(bool isVisible)
        {
            _saveLoadService?.SetTopLineVisibility(isVisible);
        }

        /// <summary>
        /// Запрашивает сохранение игры. Если кулдаун не прошел, сохранение будет отложено.
        /// </summary>
        public void RequestSave()
        {
            if (_isSaving) return;

            if (_timeSinceLastSave >= SaveCooldown)
            {
                StartCoroutine(SaveGameCoroutine());
            }
            else
            {
                _savePending = true;
            }
        }

        /// <summary>
        /// Корутина для асинхронного сохранения игры.
        /// </summary>
        private IEnumerator SaveGameCoroutine()
        {
            if (_isSaving) yield break;
            _isSaving = true;
            _savePending = false;
            _timeSinceLastSave = 0f;

            _saveLoadService?.RequestSave();

            yield return new WaitForSeconds(0.1f);

            _isSaving = false;
        }
    }
}