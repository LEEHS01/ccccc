
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
        public string unit; //����
        //public bool is_using; //���ʿ��Ѱ� ����. �ٵ� Ȯ�强 �׳� �����ؼ� ������� �ʾҳ�? �� ��¥ ���� �𸣰ڴ�
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
        ERROR,      //���� ���� �Ұ�(-1)
        NORMAL,     //����(0)
        SERIOUS,    //���(1)
        WARNING,    //�溸(2)
        CRITICAL,   //�ɰ�(3)
    }
}
