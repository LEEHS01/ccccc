using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.ExeBuild
{
    [System.Serializable]
    internal class AlarmLogModel
    {
        internal int alaidx;
        internal string aladt;
        internal string obsnm;
        internal string areanm;
        internal int hnsidx;
        internal int obsidx;
        internal int boardidx;
        internal float? alahival;
        internal float? alahihival;
        internal float? currval;
        internal string hnsnm;
        internal string turnoff_flag;
        internal string turnoff_dt;
        internal int alacode;
    }
}
