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
        //������ ����
        List<AlarmLogModel> newLogs = modelProvider.GetAlarmLogList();
        if (newLogs == null) throw new Exception("modelProvider.GetAlarmLogList()�� null�� ��ȯ�ϰ� �ֽ��ϴ�.");

        alarmLogs.Clear();
        alarmLogs.AddRange(newLogs);

        alarmLogs = alarmLogs.Where(log => 
            log.GetAlarmLevel() == StatusType.WARNING || 
            alarmLogs.Find(llog => 
                llog.sensor_id == log.sensor_id && 
                llog.GetAlarmLevel() == StatusType.WARNING) == null).ToList();

        //�α׵����� > ���ڿ�
        List<string> newAlarmTexts = TranslateLogsToTexts(this.alarmLogs);
        alarmTexts = newAlarmTexts;

        //��� ���� �ʱ�ȭ
        if (newAlarmTexts != alarmTexts) nowAlarmTextIdx = -1;
    }
    #endregion

    #region Process
    private void LabelAnimationProcess()
    {
        //������ ����� �ؽ�Ʈ ����
        string newText;

        if (alarmTexts.Count == 0) {
            newText = $"{DateTimeKst.Now.ToString("yyyy-MM-dd HH:mm")}  �˸� ���� ����";
        }
        else 
        {
            if (++nowAlarmTextIdx >= alarmTexts.Count)
                nowAlarmTextIdx = 0;

            newText = alarmTexts[nowAlarmTextIdx];
        }

        //�� ����
        TMP_Text lblOld = lblAlarmNotify[0], lblNew = lblAlarmNotify[1];

        lblOld.rectTransform.DOAnchorPos(new(0f, 15f), 0.8f);
        lblOld.DOColor(new(1, 1, 1, 0), 0.8f);

        lblNew.rectTransform.anchoredPosition = new(0f, -15f);
        lblNew.rectTransform.DOAnchorPos(Vector2.zero, 0.8f);
        lblNew.DOColor(new(1, 1, 1, 1), 0.8f);
        lblNew.text = newText;

        lblOld.transform.SetAsLastSibling();

        //Debug.Log($"LabelAnimationProcess : txtidx{nowAlarmTextIdx} alarmLogs{alarmLogs.Count} alarmTexts{alarmTexts.Count}");

        //ȸ�� ȣ��
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
            //status == StatusType.CRITICAL? "ġ��" :
            status == StatusType.WARNING ? "�溸" :
            status == StatusType.SERIOUS ? "���" : "(�� �� ����)";
        res += " �߻�";

        return res;
    }
    #endregion
}