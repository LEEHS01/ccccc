
using System.Collections.Generic;

namespace Onthesys.WebBuild
{
	[System.Serializable]
	public class SensorModel
    {
        public int board_id;
        public int sensor_id;
        public float threshold_serious;
        public float threshold_warning;
        public float threshold_critical;
        public string sensor_name;
        public int is_using;
        public int is_fixing;


        public bool isUsing => is_using == 1;
        public bool isFixing => is_fixing == 1;
    }


    [System.Serializable]
    public class SensorModelList
    {
        public List<SensorModel> items;
    }

    public enum StatusType
    {
        ERROR,      //상태 계측 불가(-1)
        NORMAL,     //정상(0)
        SERIOUS,    //경계(1)
        WARNING,    //경보(2)
        CRITICAL,   //심각(3)
    }
}
