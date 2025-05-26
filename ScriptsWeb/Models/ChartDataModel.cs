 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.ExeBuild
{
    [System.Serializable]
    internal class ChartDataModel
    {
        internal string obsdt;
        internal int? hnsidx;
        internal int? obsidx;
        internal int? boardidx;

        internal float val;
        internal float aival;
    }
}
