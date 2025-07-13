using System.Collections.Generic;
using Core;

namespace YG
{
    public partial class SavesYG
    {
        public bool isTopLineVisible = true;
        public List<CellDataSerializable> gridCells = new();
        public StatisticsModelSerializable statistics = new();
        public ActionCountersModelSerializable actionCounters = new();
        public bool isGameEverSaved;
    }
}