using System;
using Interfaces;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Реализация платформенных сервисов (покупки, реклама) для Yandex Games.
    /// Реализует IDisposable для корректной отписки от событий.
    /// </summary>
    public class YandexPlatformService : IPlatformServices, IDisposable
    {
        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<string> OnRewardVideoSuccess;

        /// <summary>
        /// Инициализирует сервис и подписывается на события Yandex SDK.
        /// </summary>
        public YandexPlatformService()
        {
            YG2.onPurchaseSuccess += OnYgPurchaseSuccess;
            YG2.onPurchaseFailed += OnYgPurchaseFailed;
            YG2.onRewardAdv += OnYgRewardVideo;
        }

        /// <summary>
        /// Корректно отписывается от всех событий Yandex SDK.
        /// </summary>
        public void Dispose()
        {
            YG2.onPurchaseSuccess -= OnYgPurchaseSuccess;
            YG2.onPurchaseFailed -= OnYgPurchaseFailed;
            YG2.onRewardAdv -= OnYgRewardVideo;
        }

        /// <summary>
        /// Инициирует покупку через Yandex SDK.
        /// </summary>
        public void Purchase(string productId)
        {
            YG2.BuyPayments(productId);
        }

        /// <summary>
        /// Показывает рекламу с вознаграждением через Yandex SDK.
        /// </summary>
        public void ShowRewardedAd(string rewardId)
        {
            YG2.RewardedAdvShow(rewardId);
        }

        private void OnYgPurchaseSuccess(string purchasedId)
        {
            OnPurchaseSuccess?.Invoke(purchasedId);
        }

        private void OnYgPurchaseFailed(string failedId)
        {
            OnPurchaseFailed?.Invoke(failedId);
        }

        private void OnYgRewardVideo(string rewardId)
        {
            OnRewardVideoSuccess?.Invoke(rewardId);
        }
    }
}