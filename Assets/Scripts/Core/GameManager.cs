// --- Измененный файл: Assets/Scripts/Core/GameManager.cs ---

using System.Collections;
using Core.Events;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private GameEvents _gameEvents;
        private ISaveLoadService _saveLoadService;

        private const float SaveCooldown = 5.0f;
        private float _timeSinceLastSave = 5.0f;
        private bool _savePending;
        private bool _isSaving;

        private void Awake()
        {
            _gameEvents = ServiceProvider.GetService<GameEvents>();
            _saveLoadService = ServiceProvider.GetService<ISaveLoadService>();
            SubscribeToEvents();
        }

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

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StartCoroutine(SaveGameCoroutine(true));
            }
        }

        private void OnApplicationQuit()
        {
            StartCoroutine(SaveGameCoroutine(true));
        }

        private void SubscribeToEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.AddListener(SetTopLineVisibilityAndSave);
            _gameEvents.onBoardCleared.AddListener(HandleBoardCleared);
        }

        private void UnsubscribeFromEvents()
        {
            if (!_gameEvents) return;
            _gameEvents.onToggleTopLine.RemoveListener(SetTopLineVisibilityAndSave);
            _gameEvents.onBoardCleared.RemoveListener(HandleBoardCleared);
        }

        private void HandleBoardCleared()
        {
            StartCoroutine(BoardClearedRoutine());
        }

        private IEnumerator BoardClearedRoutine()
        {
            yield return new WaitForSeconds(2.0f);

            var gameController = ServiceProvider.GetService<GameController>();
            gameController?.StartNewGame(false);
        }

        private void SetTopLineVisibilityAndSave(bool isVisible)
        {
            _saveLoadService?.SetTopLineVisibility(isVisible);
        }

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

        private IEnumerator SaveGameCoroutine(bool force = false)
        {
            if (_isSaving && !force) yield break;

            _isSaving = true;
            _savePending = false;
            _timeSinceLastSave = 0f;

            _saveLoadService?.RequestSave();

            yield return new WaitForSeconds(0.1f);

            _isSaving = false;
        }
    }
}