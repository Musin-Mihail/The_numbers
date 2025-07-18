namespace Interfaces
{
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