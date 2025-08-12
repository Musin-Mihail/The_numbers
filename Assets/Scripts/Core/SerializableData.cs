using System;
using Model;

namespace Core
{
    /// <summary>
    /// Сериализуемая версия данных ячейки для сохранения.
    /// </summary>
    [Serializable]
    public class CellDataSerializable
    {
        public int number;
        public int line;
        public int column;
        public bool isActive;

        /// <summary>
        /// Инициализирует новый пустой экземпляр для десериализации.
        /// </summary>
        public CellDataSerializable()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр на основе данных из модели ячейки.
        /// </summary>
        /// <param name="cellData">Данные ячейки из модели.</param>
        public CellDataSerializable(CellData cellData)
        {
            number = cellData.Number;
            line = cellData.Line;
            column = cellData.Column;
            isActive = cellData.IsActive;
        }
    }

    /// <summary>
    /// Сериализуемая версия модели статистики для сохранения.
    /// </summary>
    [Serializable]
    public class StatisticsModelSerializable
    {
        public long score;
        public int multiplier;

        /// <summary>
        /// Инициализирует новый пустой экземпляр для десериализации.
        /// </summary>
        public StatisticsModelSerializable()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр на основе данных из модели статистики.
        /// </summary>
        /// <param name="model">Модель статистики.</param>
        public StatisticsModelSerializable(StatisticsModel model)
        {
            score = model.Score;
            multiplier = model.Multiplier;
        }
    }

    /// <summary>
    /// Сериализуемая версия модели счетчиков действий для сохранения.
    /// </summary>
    [Serializable]
    public class ActionCountersModelSerializable
    {
        public int undoCount;
        public int hintCount;
        public bool areCountersDisabled;

        /// <summary>
        /// Инициализирует новый пустой экземпляр для десериализации.
        /// </summary>
        public ActionCountersModelSerializable()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр на основе данных из модели счетчиков.
        /// </summary>
        /// <param name="model">Модель счетчиков действий.</param>
        public ActionCountersModelSerializable(ActionCountersModel model)
        {
            undoCount = model.UndoCount;
            hintCount = model.HintCount;
            areCountersDisabled = model.AreCountersDisabled;
        }
    }
}