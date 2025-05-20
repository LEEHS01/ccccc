using DG.Tweening;
using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


internal class ToxinList2 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private LogData log;                      // 알람 로그 데이터
    private List<ToxinBar2> bars = new();      // 독성 바 목록
    internal RectTransform scrollContainer;      // 스크롤 컨테이너
    //internal GameObject barPrefab;               // Bar 프리팹
    internal Transform barParent;                // Bar가 추가될 부모 객체
    internal TMP_Text txtName;                   // 이름 텍스트
    internal SetTime txtTime;                    // 로그 시간 텍스트 1
    internal SetTime txtTime2;                   // 로그 시간 텍스트 2

    int obsId;
    Vector3 defaultPos;


    void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectLog);
        UiManager.Instance.Register(UiEventType.ChangeAlarmSensorList, OnLoadAlarmData);


        var barComps = scrollContainer.GetComponentsInChildren<ToxinBar2>();
        bars.AddRange(barComps);

        defaultPos = transform.position;
        gameObject.SetActive(false);
        //InitializeBars();
    }

    //private void InitializeBars()
    //{
    //    // 기존 바 제거 및 리스트 초기화
    //    foreach (Transform child in barParent)
    //    {
    //        if(child.gameObject != barPrefab)
    //            Destroy(child.gameObject);
    //    }
    //    bars.Clear();
    //}


    private void OnSelectLog(object data)
    {
        if (data is not int logIdx) return;

        log = modelProvider.GetAlarm(logIdx);
        this.obsId = log.obsId;
        txtName.text = $"{log.areaName} - {log.obsName}";
        txtTime.SetText(log.time);       
        txtTime2.SetText(log.time);

        transform.position = defaultPos;
        gameObject.SetActive(true);
    }

    private void OnLoadAlarmData(object obj)
    {
        List<ToxinData> toxinDatas = modelProvider.GetToxinsInLog();

        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;
        toxinBoard = toxinDatas.Where(item => item.boardid == 1).ToList();
        chemicalBoard = toxinDatas.Where(item => item.boardid == 2).ToList();
        qualityBoard = toxinDatas.Where(item => item.boardid == 3).ToList();

        List<ToxinData> sortedToxins = new();
        sortedToxins.AddRange(toxinBoard);
        sortedToxins.AddRange(qualityBoard);
        sortedToxins.AddRange(chemicalBoard);


        float scrollHeight = 0;

        for (int i = 0; i < sortedToxins.Count; i++)
        {
            if (i >= bars.Count)
            {
                Debug.LogWarning($"[ToxinList2] Bar index {i} is out of range");
                continue;
            }

            ToxinData toxin = sortedToxins[i];
            ToxinBar2 bar = bars[i];

            bar.SetToxinData(obsId, toxin, toxin.status);
            bar.gameObject.SetActive(toxin.on);

            if (toxin.on)
                scrollHeight += bar.GetComponent<RectTransform>().rect.height;
        }

        RectTransform rect = this.scrollContainer.GetComponent<RectTransform>();
        rect.DOSizeDelta(new Vector2(rect.rect.width, scrollHeight + 76), 0);

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollContainer);
    }


}

