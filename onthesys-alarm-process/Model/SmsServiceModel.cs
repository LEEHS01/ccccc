using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.WebBuild
{
    [System.Serializable]
    public class SmsServiceModel
    {
        public int service_id;
        public string name;
        public string phone;
        public bool is_enabled;
        //public bool isEnabled => is_enabled == 1;
        public string alarm_level;
        public string checked_time; //최근 확인 시간
        public DateTime CheckedTime => DateTime.Parse(checked_time);
        public int board_id;
        public int sensor_id;
    }

    [System.Serializable]
    public class SmsServiceModelList
    {
        public List<SmsServiceModel> items;
    }
}