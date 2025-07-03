using System;

namespace Model
{
    [Serializable]
    public class CellData
    {
        public Guid Id { get; }
        public int Number { get; }
        public int Line { get; set; }
        public int Column { get; }
        public bool IsActive { get; private set; }

        public CellData(int number, int line, int column)
        {
            Id = Guid.NewGuid();
            Number = number;
            Line = line;
            Column = column;
            IsActive = true;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}