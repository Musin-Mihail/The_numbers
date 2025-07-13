using System;
using Model;

namespace Core
{
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