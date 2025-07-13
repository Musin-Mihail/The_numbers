using System;

namespace Core
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

    /// <summary>
    /// Интерфейс для сервиса сохранения и загрузки игры.
    /// </summary>
    public interface ISaveLoadService
    {
        /// <summary>
        /// Запрашивает сохранение текущего состояния игры.
        /// </summary>
        void RequestSave();

        /// <summary>
        /// Загружает сохраненное состояние игры.
        /// </summary>
        /// <param name="onComplete">Callback, вызываемый по завершении загрузки. Параметр bool указывает на успех.</param>
        void LoadGame(Action<bool> onComplete);

        /// <summary>
        /// Устанавливает и сохраняет состояние видимости верхней строки.
        /// </summary>
        /// <param name="isVisible">True, если строка должна быть видимой.</param>
        void SetTopLineVisibility(bool isVisible);
    }

    /// <summary>
    /// Интерфейс для сервиса таблицы лидеров.
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Обновляет счет игрока в таблице лидеров.
        /// </summary>
        /// <param name="score">Новый счет.</param>
        void UpdateLeaderboard(int score);
    }
}