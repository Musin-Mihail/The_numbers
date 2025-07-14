namespace Core
{
    /// <summary>
    /// Содержит глобальные константы, используемые в игре.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Идентификатор продукта для отключения счетчиков действий (покупка в игре).
        /// </summary>
        public const string DisableCountersProductId = "disable_counters";

        /// <summary>
        /// Идентификатор для вознаграждения за просмотр рекламы (пополнение счетчиков).
        /// </summary>
        public const string RefillCountersRewardId = "refillCounters";

        /// <summary>
        /// Начальное количество доступных действий (отмена, добавление, подсказка).
        /// </summary>
        public const int InitialActionsCount = 5;

        /// <summary>
        /// Начальное количество линий, генерируемых в начале новой игры.
        /// </summary>
        public const int InitialLinesOnStart = 5;
    }
}