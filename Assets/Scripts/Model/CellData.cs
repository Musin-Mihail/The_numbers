using System;

namespace Model
{
    /// <summary>
    /// Модель данных, представляющая одну ячейку на игровой сетке.
    /// </summary>
    [Serializable]
    public class CellData
    {
        /// <summary>
        /// Уникальный идентификатор ячейки.
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Число, отображаемое в ячейке.
        /// </summary>
        public int Number { get; }
        
        /// <summary>
        /// Индекс строки, в которой находится ячейка.
        /// </summary>
        public int Line { get; set; }
        
        /// <summary>
        /// Индекс столбца, в котором находится ячейка.
        /// </summary>
        public int Column { get; }
        
        /// <summary>
        /// Указывает, является ли ячейка активной (видимой и доступной для взаимодействия).
        /// </summary>
        public bool IsActive { get; private set; }

        public CellData(int number, int line, int column)
        {
            Id = Guid.NewGuid();
            Number = number;
            Line = line;
            Column = column;
            IsActive = true;
        }

        /// <summary>
        /// Устанавливает активное состояние ячейки.
        /// </summary>
        /// <param name="isActive">Новое состояние активности.</param>
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}