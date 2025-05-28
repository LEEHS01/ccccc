using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Onthesys.WebBuild
{
    //직렬화에 영향이 있을까봐 확장메서드로 분할
    public static class AlarmLogModelExtension
    {
        public static StatusType GetAlarmLevel(this AlarmLogModel logModel) => logModel.alarm_level switch
        {
            "Error" => StatusType.ERROR,
            "Normal" => StatusType.NORMAL,
            "Serious" => StatusType.SERIOUS,
            "Warning" => StatusType.WARNING,
            "Critical" => StatusType.CRITICAL,
            _ => throw new Exception("예상 범위 밖의 인자가 제시됐습니다. AlarmLogModel.alarm_level 을 StatusType 자료형으로 파싱하는데에 실패했습니다."),
        };
        public static string ToDbString(this StatusType status) => status switch
        {
            StatusType.ERROR => "Error",
            StatusType.NORMAL => "Normal",
            StatusType.SERIOUS => "Serious",
            StatusType.WARNING => "Warning",
            StatusType.CRITICAL => "Critical",
            _ => throw new Exception("예상 범위 밖의 인자가 제시됐습니다. AlarmLogModel.alarm_level 을 StatusType 자료형으로 파싱하는데에 실패했습니다."),
        };
    }


    [System.Serializable]
    public class AlarmLogModel
    {
        public int alarm_id;
        public int board_id;
        public int sensor_id;
        public string alarm_level;
        public string occured_time;
        public string solved_time;
        public DateTime OccuredTime => DateTime.Parse(occured_time);
        public DateTime? SolvedTime()
        {
            try
            {
                return DateTime.Parse(occured_time);
            }
            catch
            {
                return null;
            }
        }
    }

    [System.Serializable]
    public class AlarmLogModelList
    {
        public List<AlarmLogModel> items;
    }
}
