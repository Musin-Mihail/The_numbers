using System;
using Core.Events;
using Model;
using UnityEngine;

namespace Core.Handlers
{
    public class PlatformBridge : IDisposable
    {
        private readonly IPlatformServices _platformServices;
        private readonly GameEvents _gameEvents;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly GameManager _gameManager;

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

        private void HandleDisableCountersConfirmed()
        {
            _platformServices?.Purchase(Constants.DisableCountersProductId);
        }

        private void HandleShowRewardedAdForRefill()
        {
            _platformServices?.ShowRewardedAd(Constants.RefillCountersRewardId);
        }

        private void OnPurchaseSuccess(string purchasedId)
        {
            if (purchasedId != Constants.DisableCountersProductId) return;

            Debug.Log($"Покупка '{purchasedId}' прошла успешно!");
            _actionCountersModel.DisableCounters();
            RaiseCountersChangedEvent();
            _gameManager?.RequestSave();
        }

        private void OnPurchaseFailed(string failedId)
        {
            Debug.LogError($"Ошибка при покупке товара '{failedId}'");
        }

        private void OnRewardVideoSuccess(string rewardId)
        {
            if (rewardId != Constants.RefillCountersRewardId) return;

            _actionCountersModel.ResetCounters();
            RaiseCountersChangedEvent();
            _gameManager?.RequestSave();
            Debug.Log("Счетчики пополнены после просмотра рекламы.");
        }

        private void RaiseCountersChangedEvent()
        {
            _gameEvents.onCountersChanged.Raise(_actionCountersModel.AreCountersDisabled
                ? (-1, -1, -1)
                : (_actionCountersModel.UndoCount, _actionCountersModel.AddNumbersCount, _actionCountersModel.HintCount));
        }
    }
}