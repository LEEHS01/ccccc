
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
        public string unit; //단위
        //public bool is_using; //불필요한거 제거. 근데 확장성 그냥 예약해서 만들라지 않았나? 난 진짜 이젠 모르겠다
        public bool is_fixing;


        //public bool isUsing => is_using;
        public bool isFixing => is_fixing;

        public float GetThresholdByStatus(StatusType status) 
        {
            switch (status) 
            {
                case StatusType.SERIOUS: return threshold_serious;
                case StatusType.WARNING: return threshold_warning;
                case StatusType.CRITICAL: return threshold_critical;
                default: return -1f;
            }
        }
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
