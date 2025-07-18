using System;

namespace Interfaces
{
    /// <summary>
    /// Интерфейс для взаимодействия с сервисами платформы (покупки, реклама).
    /// </summary>
    public interface IPlatformServices
    {
        /// <summary>
        /// Событие, вызываемое при успешной покупке.
        /// </summary>
        event Action<string> OnPurchaseSuccess;

        /// <summary>
        /// Событие, вызываемое при неудачной покупке.
        /// </summary>
        event Action<string> OnPurchaseFailed;

        /// <summary>
        /// Событие, вызываемое при успешном просмотре видео с вознаграждением.
        /// </summary>
        event Action<string> OnRewardVideoSuccess;

        /// <summary>
        /// Инициирует процесс покупки продукта.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        void Purchase(string productId);

        /// <summary>
        /// Показывает видео с вознаграждением.
        /// </summary>
        /// <param name="rewardId">Идентификатор вознаграждения.</param>
        void ShowRewardedAd(string rewardId);
    }
}