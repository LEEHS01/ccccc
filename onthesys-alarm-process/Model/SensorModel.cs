
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
        public bool is_using;
        public bool is_fixing;


        public bool isUsing => is_using;
        public bool isFixing => is_fixing;
    }


    [System.Serializable]
    public class SensorModelList
    {
        public List<SensorModel> items;
    }

    public enum StatusType
    {
        ERROR,      //���� ���� �Ұ�(-1)
        NORMAL,     //����(0)
        SERIOUS,    //���(1)
        WARNING,    //�溸(2)
        CRITICAL,   //�ɰ�(3)
    }
}
