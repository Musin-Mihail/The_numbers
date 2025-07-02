namespace Model
{
    public class StatisticsModel
    {
        public long Score { get; private set; }
        public int Multiplier { get; private set; }

        public StatisticsModel()
        {
            Reset();
        }

        public void Reset()
        {
            Score = 0;
            Multiplier = 1;
        }

        public void AddScore(long value)
        {
            Score += value;
        }

        public void IncrementMultiplier()
        {
            Multiplier++;
        }

        public void SetState(long score, int multiplier)
        {
            Score = score;
            Multiplier = multiplier;
        }
    }
}