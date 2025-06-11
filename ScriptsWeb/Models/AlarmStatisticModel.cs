using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.WebBuild
{
    [System.Serializable]
    public class AlarmStatisticModel
    {
        public AlarmStatisticModel(int sensorId, string alarmLevel, int count, string latestTime, float durationSec)
        {
            this.sensor_id = sensorId;
            this.alarm_level = alarmLevel;
            this.count = count;
            this.latest_alarm = latestTime;
            this.duration_sec = durationSec;
        }

        public int sensor_id;
        public string alarm_level;
        public int count;
        public string latest_alarm;
        public float duration_sec;

        public DateTime lastestTime => DateTime.Parse(latest_alarm).AddHours(-9);
    }

    [System.Serializable]
    public class AlarmStatisticModelList
    {
        public List<AlarmStatisticModel> items;
    }
}
