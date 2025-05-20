using DG.Tweening;
using JetBrains.Annotations;
using NUnit.Framework;
using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class PopupAlarm : MonoBehaviour 
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    Button btnClose, btnAlarmTransition;
    Image imgSignalLamp, imgSignalLight, imgIcon;
    TMP_Text lblTitle, lblSummary;//, lblAwareTransition;

    Tween intervalThread;

    List<LogData> previousLogs = new();
    LogData alarmLog = null;
    int passMins = 0;
    const float intervalValue = 60f;    //60f = 1min

    static Dictionary<ToxinStatus, String> statusSpriteDic = new();
    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    static PopupAlarm()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#3EFF00"},
            { ToxinStatus.Yellow,   "#FFF600"},
            { ToxinStatus.Red,      "#FF0000"},
            { ToxinStatus.Purple,   "#6C00E2"},
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                statusColorDic[pair.Key] = color;

        statusSpriteDic = new()
        {
            {ToxinStatus.Yellow,   "Image/ErrorIcon/Serious"},
            {ToxinStatus.Red,      "Image/ErrorIcon/Warning"},
            {ToxinStatus.Purple,   "Image/ErrorIcon/Malfunction"},
        };

    }

    private void Start()
    {
        Transform titleLine = transform.Find("TitleLine");
        lblTitle = titleLine.Find("lblTitle").GetComponent<TMP_Text>();
        btnClose = titleLine.Find("btnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(OnCloseAlarm);

        Transform titleCircle = titleLine.Find("Icon_EventPanel_TitleCircle");
        imgSignalLamp = titleCircle.Find("imgSignalLamp").GetComponentInChildren<Image>();
        imgSignalLight = titleCircle.Find("imgSignalLight").GetComponentInChildren<Image>();

        btnAlarmTransition = transform.Find("btnAlarmTransition").GetComponent<Button>();
        btnAlarmTransition.onClick.AddListener(OnClickAlarmTransition);

        lblSummary = transform.Find("lblSummary").GetComponent<TMP_Text>();
        //lblAwareTransition = transform.Find("lblAwareTransition").GetComponent<TMP_Text>();

        imgIcon = transform.Find("imgIcon").GetComponent<Image>();

        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);

        gameObject.SetActive(false);
    }

    private void OnInitiate(object obj)
    {
        previousLogs = new(modelProvider.GetAlarms());
    }

    void OnChangeAlarmList(object obj)
    {
        List<LogData> rawLogDatas, newLogDatas;

        Debug.Log("previousLogs : " + previousLogs.Count);

        //obj에서 받아 쓰는건 디버그용 코드
        rawLogDatas = obj is List<LogData> alarmLogs ? alarmLogs : modelProvider.GetAlarms();
        //rawLogDatas = modelProvider.GetAlarms();

        //Debug.Log("rawLogDatas : " + rawLogDatas.Count);
        newLogDatas = rawLogDatas.Where(item => !previousLogs.Contains(item)).ToList();

        //Debug.Log("newLogDatas : " + newLogDatas.Count);

        //조건을 충족하는 가장 최신의 알람을 찾을 때까지 순회
        for (int i = newLogDatas.Count - 1; i >= 0; i--) if (InitAlarmLog(newLogDatas[i])) break;

        previousLogs = new (rawLogDatas);

    }

    internal bool InitAlarmLog(LogData data)
    {
        ToxinStatus logStatus = data.status == 0? ToxinStatus.Purple : (ToxinStatus)data.status;

        if (logStatus <= Option.alarmThreshold) return false;

        if (this.alarmLog == data) return false;

        passMins = 0;
        alarmLog = data;
        IntervalUpdateView();

        intervalThread?.Kill();
        intervalThread = DOVirtual.DelayedCall(intervalValue,IntervalUpdateView).SetUpdate(true);
        intervalThread.SetLoops(-1, LoopType.Restart);

        //gameObject.SetActive(false);

        gameObject.SetActive(true);
        return true;
    }

    void IntervalUpdateView()
    {
        passMins++;

        //lblSummary 설정
        {
            DateTime logDt = alarmLog.time;
            string passTimeString = "#ERROR!";
            if (passMins < 60)
            {
                passTimeString = $"{passMins} 분";
            }
            else if (passMins < 60 * 24)
            {
                passTimeString = $"{passMins / 60} 시간";
            }
            else
            {
                passTimeString = $"{passMins / 60 / 24} 일";
            }

            lblSummary.text = "" +
                $"발생 지점 : {alarmLog.areaName} - {alarmLog.obsName}\n" +
                $"발생 시각 : {logDt:yy/MM/dd HH:mm}({passTimeString} 전)\n\n";

            if ((ToxinStatus)alarmLog.status == ToxinStatus.Purple)
            {
                lblSummary.text +=
                    $"설비 이상 : {"보드 " + alarmLog.boardId}";
            }
            else
            {
                lblSummary.text +=
                    $"원인 물질 : {alarmLog.hnsName}\n" +
                    $"측정 값 : {alarmLog.value.Value} / {alarmLog.value}";
            }
        }

        ToxinStatus logStatus = alarmLog.status == 0 ? ToxinStatus.Purple : (ToxinStatus)alarmLog.status;
        //alacode에 맞게 lblTitle, imgSignalLamp 변경
        switch (logStatus)
        {
            case ToxinStatus.Purple: //설비이상
                lblTitle.text = $"설비이상 발생 : {alarmLog.areaName} - {alarmLog.obsName} 보드 {alarmLog.boardId}번";
                break;
            case ToxinStatus.Yellow: //경계
                lblTitle.text = $"경계 알람 발생 : {alarmLog.areaName} - {alarmLog.obsName} {alarmLog.hnsName}";
                break;
            case ToxinStatus.Red: //경보
                lblTitle.text = $"경보 알람 발생 : {alarmLog.areaName} - {alarmLog.obsName} {alarmLog.hnsName}";
                break;
        }
        imgSignalLamp.color = statusColorDic[logStatus];
        imgSignalLight.color = imgSignalLamp.color;

        imgIcon.sprite = Resources.Load<Sprite>(statusSpriteDic[logStatus]);
    }


    private void OnClickAlarmTransition()
    {
        UiManager.Instance.Invoke(UiEventType.SelectAlarm, alarmLog);
        UiManager.Instance.Invoke(UiEventType.NavigateObs, alarmLog.obsId);
        OnCloseAlarm();
    }
    private void OnCloseAlarm()
    {
        gameObject.SetActive(false);
    }
}
