using System;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class GameData
    {
        public List<CellDataSerializable> gridCells;
        public StatisticsModelSerializable statistics;
        public ActionCountersModelSerializable actionCounters;
        public bool isTopLineVisible;
    }

    [Serializable]
    public class CellDataSerializable
    {
        public int number;
        public int line;
        public int column;
        public bool isActive;

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

        public ActionCountersModelSerializable(ActionCountersModel model)
        {
            undoCount = model.UndoCount;
            addNumbersCount = model.AddNumbersCount;
            hintCount = model.HintCount;
            areCountersDisabled = model.AreCountersDisabled;
        }
    }


    public static class SaveLoadService
    {
        private const string GameDataKey = "GameData";

        public static void SaveGame(GameData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(GameDataKey, json);
            PlayerPrefs.Save();
        }

        public static GameData LoadGame()
        {
            if (PlayerPrefs.HasKey(GameDataKey))
            {
                var json = PlayerPrefs.GetString(GameDataKey);
                var data = JsonUtility.FromJson<GameData>(json);
                return data;
            }

            return null;
        }

        public static void DeleteSavedData()
        {
            PlayerPrefs.DeleteKey(GameDataKey);
        }
    }
}