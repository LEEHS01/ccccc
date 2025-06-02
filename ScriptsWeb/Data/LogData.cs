using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace Onthesys.ExeBuild
{
    internal class LogData
    {
        internal int obsId;
        internal int boardId;
        internal DateTime time;
        internal string areaName;
        internal string obsName;
        internal int hnsId;
        internal string hnsName;
        internal int status;
        internal float? value;
        internal int idx;
        internal float serious, warning;

        internal LogData(int obsid, int boardid, string areaName, string obsName, int hnsId, string hnsName, DateTime dt, int status, float? val, int idx, float serious, float warning)
        {
            this.obsId = obsid;
            this.boardId = boardid;
            this.areaName = areaName;
            this.obsName = obsName;
            this.hnsId = hnsId;
            this.hnsName = hnsName;
            this.time = dt;
            this.status = status;
            this.value = val;
            this.idx = idx;
            this.serious = serious;
            this.warning = warning;
        }

        internal static LogData FromAlarmLogModel(AlarmLogModel item) => 
            new LogData(
                item.obsidx, item.boardidx, item.areanm, item.obsnm, item.hnsidx, item.hnsnm, 
                Convert.ToDateTime(item.aladt), item.alacode, item.currval, item.alaidx,
                item.alahival.HasValue ? item.alahival.Value : 0f,
                item.alahihival.HasValue ? item.alahihival.Value : 0f);
    }

}


