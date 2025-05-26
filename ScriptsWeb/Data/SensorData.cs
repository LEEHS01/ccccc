using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{
    internal class SensorData
    {
        internal DateTime Timestamp { get; set; }
        internal ToxinData ToxinInfo { get; set; }

        internal SensorData(DateTime timestamp, ToxinData toxinInfo)
        {
            Timestamp = timestamp;
            ToxinInfo = toxinInfo;
        }
    }
}
