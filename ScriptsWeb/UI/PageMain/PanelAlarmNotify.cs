using DG.Tweening;
using NUnit.Framework;
using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PanelAlarmNotify : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    //Data
    List<AlarmLogModel> alarmLogs = new();
    List<string> alarmTexts = new();
    int nowAlarmTextIdx = -1;

    //Component(as Func)
    List<TMP_Text> lblAlarmNotify => GetComponentsInChildren<TMP_Text>().ToList();

    #region Initiating
    private void Start()
    {
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmLog, OnChangeAlarmLog);
    }

    #endregion

    #region EventListener
    private void OnInitiate(object obj)
    {
        DOVirtual.DelayedCall(3, LabelAnimationProcess);
    }
    private void OnChangeAlarmLog(object obj)
    {
        //데이터 수취
        List<AlarmLogModel> newLogs = modelProvider.GetAlarmLogList();
        if (newLogs == null) throw new Exception("modelProvider.GetAlarmLogList()가 null을 반환하고 있습니다.");

        alarmLogs.Clear();
        alarmLogs.AddRange(newLogs);

        alarmLogs = alarmLogs.Where(log => 
            log.GetAlarmLevel() == StatusType.WARNING || 
            alarmLogs.Find(llog => 
                llog.sensor_id == log.sensor_id && 
                llog.GetAlarmLevel() == StatusType.WARNING) == null).ToList();

        //로그데이터 > 문자열
        List<string> newAlarmTexts = TranslateLogsToTexts(this.alarmLogs);
        alarmTexts = newAlarmTexts;

        //재생 순서 초기화
        if (newAlarmTexts != alarmTexts) nowAlarmTextIdx = -1;
    }
    #endregion

    #region Process
    private void LabelAnimationProcess()
    {
        //다음에 출력할 텍스트 선정
        string newText;

        if (alarmTexts.Count == 0) {
            newText = $"{DateTimeKst.Now.ToString("yyyy-MM-dd HH:mm")}  알림 사항 없음";
        }
        else 
        {
            if (++nowAlarmTextIdx >= alarmTexts.Count)
                nowAlarmTextIdx = 0;

            newText = alarmTexts[nowAlarmTextIdx];
        }

        //라벨 스왑
        TMP_Text lblOld = lblAlarmNotify[0], lblNew = lblAlarmNotify[1];

        lblOld.rectTransform.DOAnchorPos(new(0f, 15f), 0.8f);
        lblOld.DOColor(new(1, 1, 1, 0), 0.8f);

        lblNew.rectTransform.anchoredPosition = new(0f, -15f);
        lblNew.rectTransform.DOAnchorPos(Vector2.zero, 0.8f);
        lblNew.DOColor(new(1, 1, 1, 1), 0.8f);
        lblNew.text = newText;

        lblOld.transform.SetAsLastSibling();

        //Debug.Log($"LabelAnimationProcess : txtidx{nowAlarmTextIdx} alarmLogs{alarmLogs.Count} alarmTexts{alarmTexts.Count}");

        //회귀 호출
        DOVirtual.DelayedCall(3, LabelAnimationProcess);
    }
    #endregion
    

    #region Function
    List<string> TranslateLogsToTexts(List<AlarmLogModel> alarmLogs) => alarmLogs.Select(alarmLog => TranslateLogToText(alarmLog)).ToList();

    string TranslateLogToText(AlarmLogModel alarmLog)
    {
        SensorModel sensorModel = modelProvider.GetSensor(1,alarmLog.sensor_id);
        StatusType status = alarmLog.GetAlarmLevel();

        string res = "";
        res += alarmLog.OccuredTime.ToString("yyyy/MM/dd HH:mm ");
        res += $"[{sensorModel.sensor_name}] ";
        res += 
            //status == StatusType.CRITICAL? "치명" :
            status == StatusType.WARNING ? "경보" :
            status == StatusType.SERIOUS ? "경계" : "(알 수 없음)";
        res += " 발생";

        return res;
    }
    #endregion
}