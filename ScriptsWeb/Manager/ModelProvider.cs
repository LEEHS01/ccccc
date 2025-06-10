using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.WebBuild
{
    interface ModelProvider
    {
        public SensorModel GetSensor(int boardId, int sensorId);
        public List<SensorModel> GetSensors();
        public List<MeasureModel> GetMeasureRecentList();
        public MeasureModel GetMeasureRecentBySensor(int boardId, int sensorId);
        public List<MeasureModel> GetMeasureLogList();
        public List<MeasureModel> GetMeasureHistoryList();
        public List<MeasureModel> GetMeasureLogBySensor(int boardId, int sensorId);
        public List<AlarmLogModel> GetAlarmLogList();
        public StatusType GetStatusBySensor(int boardId, int sensorId);
        public StatusType GetStatusBySensorAndValue(int boardId, int sensorId, float value);


        public List<SmsServiceModel> GetSmsServices();
        public SmsServiceModel GetSmsServiceById(int serviceId);
        //public List<CorrelationModel> GetCorrelations();
        //public List<CorrelationModel> GetCorrelationBySensor(int boardId, int sensorId);
        //public List<MeasureModel> GetMeasureInferenceList();
        //public List<MeasureModel> GetMeasureDenoisedList();
    }
}
