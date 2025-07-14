using System.Collections;
using Core.Events;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Интерфейс для уведомления о завершении начальной загрузки.
    /// </summary>
    public interface IInitialLoadNotifier
    {
        /// <summary>
        /// Уведомляет, что начальная загрузка данных завершена.
        /// </summary>
        void NotifyInitialLoadComplete();
    }

    /// <summary>
    /// Управляет глобальным состоянием игры, таким как сохранение прогресса,
    /// обработка паузы и выхода из приложения.
    /// </summary>
    public class GameManager : MonoBehaviour, IInitialLoadNotifier
    {
        private GameEvents _gameEvents;
        private ISaveLoadService _saveLoadService;

        private const float SaveCooldown = 5.0f;
        private float _timeSinceLastSave = 5.0f;
        private bool _savePending;
        private bool _isSaving;

        private bool _isInitialLoadComplete;

        /// <summary>
        /// Инициализация, получение сервисов и подписка на события.
        /// </summary>
        private void Awake()
        {
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            _saveLoadService = ServiceProvider.GetService<ISaveLoadService>();
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
        /// Сохраняет игру при постановке приложения на паузу.
        /// </summary>
        /// <param name="pauseStatus">Статус паузы.</param>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StartCoroutine(SaveGameCoroutine(true));
            }
        }

        /// <summary>
        /// Сохраняет игру при выходе из приложения.
        /// </summary>
        private void OnApplicationQuit()
        {
            StartCoroutine(SaveGameCoroutine(true));
        }

        /// <summary>
        /// Подписывается на игровые события.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.AddListener(SetTopLineVisibilityAndSave);
            _gameEvents.onBoardCleared.AddListener(HandleBoardCleared);
        }

        /// <summary>
        /// Отписывается от игровых событий.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.RemoveListener(SetTopLineVisibilityAndSave);
            _gameEvents.onBoardCleared.RemoveListener(HandleBoardCleared);
        }

        /// <summary>
        /// Обрабатывает событие полной очистки доски.
        /// </summary>
        private void HandleBoardCleared()
        {
            StartCoroutine(BoardClearedRoutine());
        }

        /// <summary>
        /// Корутина, которая запускает новую игру после небольшой задержки после очистки доски.
        /// </summary>
        private IEnumerator BoardClearedRoutine()
        {
            yield return new WaitForSeconds(2.0f);

            var gameController = ServiceProvider.GetService<GameController>();
            gameController?.StartNewGame(false);
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
        /// Устанавливает флаг, что начальная загрузка завершена.
        /// Вызывается из GameBootstrap после успешной загрузки или создания новой игры.
        /// </summary>
        public void NotifyInitialLoadComplete()
        {
            _isInitialLoadComplete = true;
            Debug.Log("Начальная загрузка завершена. Сохранение разблокировано.");
        }

        /// <summary>
        /// Запрашивает сохранение игры. Если кулдаун не прошел, сохранение будет отложено.
        /// </summary>
        public void RequestSave()
        {
            if (!_isInitialLoadComplete)
            {
                Debug.LogWarning("Сохранение заблокировано: начальная загрузка еще не завершена.");
                return;
            }

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
        /// <param name="force">Если true, сохранение произойдет немедленно, игнорируя статус _isSaving.</param>
        private IEnumerator SaveGameCoroutine(bool force = false)
        {
            if (_isSaving && !force) yield break;

            if (!_isInitialLoadComplete)
            {
                Debug.LogWarning("Принудительное сохранение заблокировано: начальная загрузка еще не завершена.");
                yield break;
            }

            _isSaving = true;
            _savePending = false;
            _timeSinceLastSave = 0f;

            _saveLoadService?.RequestSave();

            yield return new WaitForSeconds(0.1f);

            _isSaving = false;
        }
    }
}