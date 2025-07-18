using System;
using Core.Events;
using Interfaces;
using Model;
using UnityEngine;

namespace Core.Handlers
{
    /// <summary>
    /// Мост между игровыми событиями и сервисами платформы (покупки, реклама).
    /// </summary>
    public class PlatformBridge : IDisposable
    {
        private readonly IPlatformServices _platformServices;
        private readonly GameEvents _gameEvents;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly GameManager _gameManager;

        /// <summary>
        /// Инициализирует мост с необходимыми сервисами и подписывается на события.
        /// </summary>
        public PlatformBridge(
            IPlatformServices platformServices,
            GameEvents gameEvents,
            ActionCountersModel actionCountersModel,
            GameManager gameManager)
        {
            _platformServices = platformServices;
            _gameEvents = gameEvents;
            _actionCountersModel = actionCountersModel;
            _gameManager = gameManager;

            SubscribeToGameEvents();
            SubscribeToPlatformEvents();
        }

        /// <summary>
        /// Отписывается от всех событий.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromGameEvents();
            UnsubscribeFromPlatformEvents();
        }

        private void SubscribeToGameEvents()
        {
            _gameEvents.onDisableCountersConfirmed.AddListener(HandleDisableCountersConfirmed);
            _gameEvents.onShowRewardedAdForRefill.AddListener(HandleShowRewardedAdForRefill);
        }

        private void UnsubscribeFromGameEvents()
        {
            _gameEvents.onDisableCountersConfirmed.RemoveListener(HandleDisableCountersConfirmed);
            _gameEvents.onShowRewardedAdForRefill.RemoveListener(HandleShowRewardedAdForRefill);
        }

        private void SubscribeToPlatformEvents()
        {
            if (_platformServices == null) return;
            _platformServices.OnPurchaseSuccess += OnPurchaseSuccess;
            _platformServices.OnPurchaseFailed += OnPurchaseFailed;
            _platformServices.OnRewardVideoSuccess += OnRewardVideoSuccess;
        }

        private void UnsubscribeFromPlatformEvents()
        {
            if (_platformServices == null) return;
            _platformServices.OnPurchaseSuccess -= OnPurchaseSuccess;
            _platformServices.OnPurchaseFailed -= OnPurchaseFailed;
            _platformServices.OnRewardVideoSuccess -= OnRewardVideoSuccess;
        }

        /// <summary>
        /// Обрабатывает подтверждение покупки отключения счетчиков.
        /// </summary>
        private void HandleDisableCountersConfirmed()
        {
            _platformServices?.Purchase(GameConstants.DisableCountersProductId);
        }

        /// <summary>
        /// Обрабатывает запрос на показ рекламы для пополнения счетчиков.
        /// </summary>
        private void HandleShowRewardedAdForRefill()
        {
            _platformServices?.ShowRewardedAd(GameConstants.RefillCountersRewardId);
        }

        /// <summary>
        /// Вызывается при успешной покупке на платформе.
        /// </summary>
        private void OnPurchaseSuccess(string purchasedId)
        {
            if (purchasedId != GameConstants.DisableCountersProductId) return;

            Debug.Log($"Покупка '{purchasedId}' прошла успешно!");
            _actionCountersModel.DisableCounters();
            _gameManager?.RequestSave();
        }

        /// <summary>
        /// Вызывается при ошибке покупки на платформе.
        /// </summary>
        private void OnPurchaseFailed(string failedId)
        {
            Debug.LogError($"Ошибка при покупке товара '{failedId}'");
        }

        /// <summary>
        /// Вызывается при успешном просмотре рекламы с вознаграждением.
        /// </summary>
        private void OnRewardVideoSuccess(string rewardId)
        {
            if (rewardId != GameConstants.RefillCountersRewardId) return;
            _actionCountersModel.ResetCounters();
            _gameManager?.RequestSave();
            Debug.Log("Счетчики пополнены после просмотра рекламы.");
        }
    }
}