namespace Core
{
    /// <summary>
    /// Содержит константы, связанные с игровой механикой и представлением.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Текущая версия игры для системы оповещений об обновлениях.
        /// Увеличивайте это число (например, до 3, 4 и т.д.) при каждом новом обновлении.
        /// </summary>
        public const int GameVersion = 1;

        /// <summary>
        /// Идентификатор продукта для отключения счетчиков действий (покупка в игре).
        /// </summary>
        public const string DisableCountersProductId = "disable_counters";

        /// <summary>
        /// Идентификатор для вознаграждения за просмотр рекламы (пополнение счетчиков).
        /// </summary>
        public const string RefillCountersRewardId = "refillCounters";

        /// <summary>
        /// Имя таблицы лидеров для Yandex Games.
        /// </summary>
        public const string LeaderboardName = "TotalScore";

        /// <summary>
        /// Начальное количество доступных действий (отмена, добавление, подсказка).
        /// </summary>
        public const int InitialActionsCount = 5;

        /// <summary>
        /// Начальное количество линий, генерируемых в начале новой игры.
        /// </summary>
        public const int InitialLinesOnStart = 5;
        /// <summary>
        /// Количество ячеек в ширину сетки.
        /// </summary>
        public const int QuantityByWidth = 10;

        /// <summary>
        /// Отступ для элементов сетки.
        /// </summary>
        public const int Indent = 10;

        /// <summary>
        /// Продолжительность анимации перемещения ячейки.
        /// </summary>
        public const float CellMoveDuration = 0.3f;

        /// <summary>
        /// Размер ячейки.
        /// </summary>
        public const float CellSize = 105f;
    }
}