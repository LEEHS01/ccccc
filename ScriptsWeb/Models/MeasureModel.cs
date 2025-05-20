
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Onthesys.WebBuild
{
    [System.Serializable]
    public class MeasureModel
    {
        public int board_id;
        public int sensor_id;
        public string measured_time;
        public float measured_value;
        public DateTime MeasuredTime => DateTime.Parse(measured_time);
    }


    [System.Serializable]
    public class MeasureModelList
    {
        public List<MeasureModel> items;
    }
}
