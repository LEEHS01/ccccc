using System;
using System.Collections.Generic;

namespace Onthesys.WebBuild
{
    [System.Serializable]
    public class MeasureModel
    {
        public int board_id;
        public int sensor_id;
        public float measured_value;
        public string measured_time;
        public DateTime MeasuredTime => DateTimeKst.Parse(measured_time);
    }


    [System.Serializable]
    public class MeasureModelList
    {
        public List<MeasureModel> items;
    }
}
