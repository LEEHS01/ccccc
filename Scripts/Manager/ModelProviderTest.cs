using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Onthesys.ExeBuild
{
    internal class ModelProviderTest : ModelProvider
    {
        internal ModelProviderTest(ModelProvider original)
        {
            modelProvider = original;
            toxinsQuality.AddRange(new List<ToxinData>() {
            new ToxinData(new HnsResourceModel(){
                alahihisec = 10f,
                alahihival = 50f,
                alahival = 40f,
                boardidx = 3,
                hnsidx = 1,
                hnsnm = "용존 산소량(DO)",
                inspectionflag = "0",
                obsidx = 1,
                useyn = "1",
            }),
            new ToxinData(new HnsResourceModel(){
                alahihisec = 10f,
                alahihival = 7.5f,
                alahival = 7f,
                boardidx = 3,
                hnsidx = 2,
                hnsnm = "산성도(pH)",
                inspectionflag = "0",
                obsidx = 1,
                useyn = "1",
            }),
            new ToxinData(new HnsResourceModel(){
                alahihisec = 10f,
                alahihival = 30f,
                alahival = 20f,
                boardidx = 3,
                hnsidx = 3,
                hnsnm = "전기전도도(EC)",
                inspectionflag = "0",
                obsidx = 1,
                useyn = "1",
            }),
            new ToxinData(new HnsResourceModel(){
                alahihisec = 30f,
                alahihival = 30f,
                alahival = 25f,
                boardidx = 3,
                hnsidx = 4,
                hnsnm = "온도(TEMP)",
                inspectionflag = "0",
                obsidx = 1,
                useyn = "1",
            }),
        });
            toxinsQuality.ForEach(item => item.CreateRandomValues());

            MakingTestvaluesRoutine();
        }
        List<ToxinData> toxinsQuality = new();

        ModelProvider modelProvider;

        List<ToxinData> GetHighjackedToxins(List<ToxinData> originalToxins)
        {
            //Debug.LogError("    List<ToxinData> GetHighjackedToxins()\r\n");

            if (originalToxins.Count == 0) return originalToxins;

            List<ToxinData> newList = new();
            newList.AddRange(originalToxins);
            newList.AddRange(toxinsQuality);

            return newList;
        }

        void MakingTestvaluesRoutine()
        {
            //Debug.LogError("MakingTestvaluesRoutine");
            toxinsQuality.ForEach(item => item.UpdateValue(null));
            DOVirtual.DelayedCall(1f, MakingTestvaluesRoutine);
        }




    public ObsData GetObs(int obsId)
        {
            return modelProvider.GetObs(obsId);
        }
    public List<ObsData> GetObss()
        {
            return modelProvider.GetObss();
        }
    public List<ObsData> GetObssByAreaId(int areaId)
        {
            return modelProvider.GetObssByAreaId(areaId);
        }
    public ToxinStatus GetObsStatus(int obsId)
        {
            return modelProvider.GetObsStatus(obsId);
        }
    public ObsData GetObsByName(string obsName)
        {
            return modelProvider.GetObsByName(obsName);
        }
    public List<AreaData> GetAreas()
        {
            return modelProvider.GetAreas();
        }
    public AreaData GetArea(int areaId)
        {
            return modelProvider.GetArea(areaId);
        }
    public ToxinStatus GetAreaStatus(int areaId)
        {
            return modelProvider.GetAreaStatus(areaId);
        }
    public AreaData GetAreaByName(string areaName)
        {
            return modelProvider.GetAreaByName(areaName);
        }
    public ToxinData GetToxin(int boardId, int sensorId)
        {
            return GetHighjackedToxins(modelProvider.GetToxins()).Find(item => item.boardid == boardId && item.hnsid == sensorId);
        }
    public List<ToxinData> GetToxins()
        {
            return GetHighjackedToxins(modelProvider.GetToxins());
        }
    public List<ToxinData> GetToxinsInLog()
        {
            return GetHighjackedToxins(modelProvider.GetToxinsInLog());
        }
    public List<ToxinData> GetToxinsSetting()
        {
            return GetHighjackedToxins(modelProvider.GetToxinsSetting());
        }
    public List<LogData> GetAlarms()
        {
            return modelProvider.GetAlarms();
        }
    public LogData GetAlarm(int alarmId)
        {
            return modelProvider.GetAlarm(alarmId);
        }
    public List<(int areaId, int count)> GetAlarmMonthly()
        {
            return modelProvider.GetAlarmMonthly();
        }
    public List<(int areaId, AlarmCount counts)> GetAlarmYearly()
        {
            return modelProvider.GetAlarmYearly();
        }
    public List<AlarmSummaryModel> GetAlarmSummary()
        {
            return modelProvider.GetAlarmSummary();
        }
    public AlarmCount GetObsStatusCountByAreaId(int areaId)
        {
            return modelProvider.GetObsStatusCountByAreaId(areaId);
        }
    public ToxinStatus GetSensorStatus(int obsId, int boardId, int sensorId)
        {
            return modelProvider.GetSensorStatus(obsId, boardId, sensorId);
        }
    }
}