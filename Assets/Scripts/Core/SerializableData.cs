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

        public CellDataSerializable()
        {
        }

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

        public StatisticsModelSerializable()
        {
        }

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
        public int addNumbersCount;
        public int hintCount;
        public bool areCountersDisabled;

        public ActionCountersModelSerializable()
        {
        }

        public ActionCountersModelSerializable(ActionCountersModel model)
        {
            undoCount = model.UndoCount;
            addNumbersCount = model.AddNumbersCount;
            hintCount = model.HintCount;
            areCountersDisabled = model.AreCountersDisabled;
        }
    }
}