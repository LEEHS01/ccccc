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


internal class ToxinList3 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    internal RectTransform scrollContainer;
    internal TMP_Text txtName;

    List<ArcBar> allItems = new(), toxinItems = new(), qualityItems = new(), chemicalItems = new();

    int obsId = -1;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.ChangeSensorList, OnLoadSetting);
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnLoadRecentMeasure);
        UiManager.Instance.Register(UiEventType.CommitSensorUsing, OnCommitSensorUsing);
        
        Transform scrollContent = transform.Find("Content");

        toxinItems = scrollContent.Find("gridsUpper").Find("Grid_Toxin").GetComponentsInChildren<ArcBar>().ToList();
        qualityItems = scrollContent.Find("gridsUpper").Find("Grid_Water_Quality").GetComponentsInChildren<ArcBar>().ToList();
        chemicalItems = scrollContent.Find("Grid_Chemicals ").GetComponentsInChildren<ArcBar>().ToList();

        allItems.AddRange(toxinItems);
        allItems.AddRange(qualityItems);
        allItems.AddRange(chemicalItems);
    }

    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        ObsData obs = modelProvider.GetObs(obsId);
        this.obsId = obsId;
        txtName.text = $"{obs.areaName} - {obs.obsName} 실시간 상태";
    }

    private void OnLoadRecentMeasure(object obj)
    {
        List<ToxinData> toxinDatas = modelProvider.GetToxins();

        if (toxinDatas.Count == 0) return;

        allItems.ForEach(bar => bar.SetAmount());
    }

    private void OnLoadSetting(object data)
    {
        List<ToxinData> toxinDatas = modelProvider.GetToxins();

        ApplySensorList(toxinDatas);
    }
    private void OnCommitSensorUsing(object obj)
    {
        if (obj is not (int obsId, int boardId, int sensorId, bool isUsing)) return;

        if (this.obsId != obsId) return;

        //List<ObsMonitoringItem> tItems = new[] { toxinItems, chemicalItems, qualityItems }[boardId-1];
        
        //new[] { toxinItems ?? new(), chemicalItems ?? new(), qualityItems ?? new() }?[boardId - 1]?[sensorId - 1]?.gameObject?.SetActive(isUsing);

        List<ArcBar> tItems = null;
        switch (boardId)
        {
            case 1: tItems = toxinItems; break;
            case 2: tItems = chemicalItems; break;
            case 3: tItems = qualityItems; break;
        }
        if (tItems is null) return;

        ArcBar tItem = null;
        tItem = tItems[sensorId - 1];
        if (tItem is null) return;

        //아이템 활성화
        tItem.gameObject.SetActive(isUsing);
        //레이아웃 갱신
        RectTransform rt = tItem.transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }


    void ApplySensorList(List<ToxinData> toxins)
    {
        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;

        toxinBoard = toxins.Where(item => item.boardid == 1).ToList();
        chemicalBoard = toxins.Where(item => item.boardid == 2).ToList();
        qualityBoard = toxins.Where(item => item.boardid == 3).ToList();

        ApplySensorListBoard(toxinBoard, toxinItems);
        ApplySensorListBoard(chemicalBoard, chemicalItems);
        ApplySensorListBoard(qualityBoard, qualityItems);
    }

    void ApplySensorListBoard(List<ToxinData> toxinsInBoard, List<ArcBar> items)
    {
        if (items.Count == 0) throw new Exception("ObsMonitoring - ApplySensorListBoard : 발견한 요소의 수가 0입니다.");

        //아이템 추가가 필요
        if (toxinsInBoard.Count > items.Count)
        {
            int needToAddCount = toxinsInBoard.Count - items.Count;
            Transform itemsParent = items[0].transform.parent;
            GameObject itemPrefab = items[0].gameObject;

            for (int i = 0; i < needToAddCount; i++)
            {
                GameObject obj = Instantiate(itemPrefab, itemsParent);
                ArcBar newItem = obj.GetComponent<ArcBar>();
                items.Add(newItem);
            }
        }

        //전체 아이템 순회
        for (int i = 0; i < items.Count; i++)
        {
            ArcBar item = items[i];
            ToxinData toxin = toxinsInBoard[i];

            //빈자리라면 비활성화
            if (i + 1 > toxinsInBoard.Count)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.SetValue(toxin);
        }

        //크기 초기화
        RectTransform rt = items[0].transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }


}

