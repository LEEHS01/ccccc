using Onthesys.WebBuild;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.WebBuild
{
    //직렬화에 영향이 있을까봐 확장메서드로 분할  
    public static class AlarmLogModelExtension
    {
        public static StatusType GetAlarmLevel(this AlarmLogModel logModel)
        {
            switch (logModel.alarm_level)
            {
                case "Error":
                    return StatusType.ERROR;
                case "Normal":
                    return StatusType.NORMAL;
                case "Serious":
                    return StatusType.SERIOUS;
                case "Warning":
                    return StatusType.WARNING;
                case "Critical":
                    return StatusType.CRITICAL;
                case "ERROR":
                    return StatusType.ERROR;
                case "NORMAL":
                    return StatusType.NORMAL;
                case "SERIOUS":
                    return StatusType.SERIOUS;
                case "WARNING":
                    return StatusType.WARNING;
                case "CRITICAL":
                    return StatusType.CRITICAL;
                default:
                    throw new Exception("예상 범위 밖의 인자가 제시됐습니다. AlarmLogModel.alarm_level 을 StatusType 자료형으로 파싱하는데에 실패했습니다.");
            }
        }
        public static StatusType GetAlarmLevel(this SmsServiceModel smsServiceModel)
        {
            switch (smsServiceModel.alarm_level)
            {
                case "Error":
                    return StatusType.ERROR;
                case "Normal":
                    return StatusType.NORMAL;
                case "Serious":
                    return StatusType.SERIOUS;
                case "Warning":
                    return StatusType.WARNING;
                case "Critical":
                    return StatusType.CRITICAL;
                case "ERROR":
                    return StatusType.ERROR;
                case "NORMAL":
                    return StatusType.NORMAL;
                case "SERIOUS":
                    return StatusType.SERIOUS;
                case "WARNING":
                    return StatusType.WARNING;
                case "CRITICAL":
                    return StatusType.CRITICAL;
                default:
                    throw new Exception("예상 범위 밖의 인자가 제시됐습니다. AlarmLogModel.alarm_level 을 StatusType 자료형으로 파싱하는데에 실패했습니다.");
            }
        }
        public static StatusType GetAlarmLevel(this AlarmStatisticModel alaStatModel)
        {
            switch (alaStatModel.alarm_level)
            {
                case "Error":
                    return StatusType.ERROR;
                case "Normal":
                    return StatusType.NORMAL;
                case "Serious":
                    return StatusType.SERIOUS;
                case "Warning":
                    return StatusType.WARNING;
                case "Critical":
                    return StatusType.CRITICAL;
                case "ERROR":
                    return StatusType.ERROR;
                case "NORMAL":
                    return StatusType.NORMAL;
                case "SERIOUS":
                    return StatusType.SERIOUS;
                case "WARNING":
                    return StatusType.WARNING;
                case "CRITICAL":
                    return StatusType.CRITICAL;
                default:
                    throw new Exception("예상 범위 밖의 인자가 제시됐습니다. AlarmLogModel.alarm_level 을 StatusType 자료형으로 파싱하는데에 실패했습니다.");
            }
        }

        public static string ToDbString(this StatusType status)
        {
            switch (status)
            {
                case StatusType.ERROR:
                    return "Error";
                case StatusType.NORMAL:
                    return "Normal";
                case StatusType.SERIOUS:
                    return "Serious";
                case StatusType.WARNING:
                    return "Warning";
                case StatusType.CRITICAL:
                    return "Critical";
            }
            throw new Exception("ToDbString - 사전에 정의되지 않은 StatusType 유형입니다.");
        }
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
        public DateTime OccuredTime => DateTimeKst.ParseRaw(occured_time);
        public DateTime? SolvedTime()
        {
            try
            {
                return DateTimeKst.ParseRaw(occured_time);
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
