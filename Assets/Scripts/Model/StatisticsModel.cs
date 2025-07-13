namespace Model
{
    /// <summary>
    /// Модель данных для хранения статистики игрока, такой как счет и множитель очков.
    /// </summary>
    public class StatisticsModel
    {
        /// <summary>
        /// Текущий счет игрока.
        /// </summary>
        public long Score { get; private set; }
        
        /// <summary>
        /// Текущий множитель очков.
        /// </summary>
        public int Multiplier { get; private set; }

        public StatisticsModel()
        {
            Reset();
        }

        /// <summary>
        /// Сбрасывает статистику к начальным значениям.
        /// </summary>
        public void Reset()
        {
            Score = 0;
            Multiplier = 1;
        }

        /// <summary>
        /// Добавляет значение к текущему счету.
        /// </summary>
        /// <param name="value">Значение для добавления.</param>
        public void AddScore(long value)
        {
            Score += value;
        }

        /// <summary>
        /// Увеличивает множитель на единицу.
        /// </summary>
        public void IncrementMultiplier()
        {
            Multiplier++;
        }

        /// <summary>
        /// Устанавливает состояние статистики (используется при загрузке или отмене).
        /// </summary>
        /// <param name="score">Новый счет.</param>
        /// <param name="multiplier">Новый множитель.</param>
        public void SetState(long score, int multiplier)
        {
            Score = score;
            Multiplier = multiplier;
        }
    }
}