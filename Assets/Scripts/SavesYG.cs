using System.Collections.Generic;
using Core;

namespace YG
{
    public partial class SavesYG
    {
        public bool isTopLineVisible = true;
        public List<CellDataSerializable> gridCells = new List<CellDataSerializable>();
        public StatisticsModelSerializable statistics = new StatisticsModelSerializable();
        public ActionCountersModelSerializable actionCounters = new ActionCountersModelSerializable();
        public bool isGameEverSaved = false;
    }
}