using DG.Tweening;
using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

internal class ObsMonitoring : MonoBehaviour
{
    
   ModelProvider modelProvider => UiManager.Instance.modelProvider;

    Vector3 defaultPos;
    Button btnSetting;

    TMP_Text lblStatus;
    Image imgSingalLamp;
    List<ObsMonitoringItem> allItems = new(), toxinItems = new(), qualityItems = new(), chemicalItems = new();

    int obsId;

    static Dictionary<ToxinStatus, Color> statusColorDic = new();
    //static Dictionary<AreaData.AreaType, Sprite> areaSpriteDic = new();
    static ObsMonitoring()
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
    }
    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);

        UiManager.Instance.Register(UiEventType.ChangeSensorList, OnChangeSensorList);
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine);
        UiManager.Instance.Register(UiEventType.ChangeSensorStatus, OnChangeSensorStatus);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
        UiManager.Instance.Register(UiEventType.CommitSensorUsing, OnCommitSensorUsing);

        Transform scrollContent = transform.Find("Scroll").Find("Content");

        toxinItems = scrollContent.Find("lstToxin").GetComponentsInChildren<ObsMonitoringItem>().ToList();
        qualityItems = scrollContent.Find("lstQuality").GetComponentsInChildren<ObsMonitoringItem>().ToList();
        chemicalItems = scrollContent.Find("lstChemical").GetComponentsInChildren<ObsMonitoringItem>().ToList();

        btnSetting = transform.Find("Btn_Setting").GetComponent<Button>();
        btnSetting.onClick.AddListener(OnClickSetting);

        lblStatus = transform.Find("lblStatus").GetComponent<TMP_Text>();

        imgSingalLamp = transform.Find("Icon_EventPanel_TitleCircle").Find("Icon_SignalLamp").GetComponent<Image>();


        defaultPos = transform.position;


        allItems.AddRange(toxinItems);
        allItems.AddRange(qualityItems);
        allItems.AddRange(chemicalItems);
    }

    private void OnChangeAlarmList(object obj)
    {
        ToxinStatus status = modelProvider.GetObsStatus(obsId);
        SetTitleStatus(status);
    }

    private void OnNavigateArea(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }
    private void OnNavigateHome(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }
    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        this.obsId = obsId;
        SetAnimation(defaultPos + new Vector3(-575f, 0f), 1f);

        ToxinStatus status = modelProvider.GetObsStatus(obsId);
        SetTitleStatus(status);
    }
    private void OnCommitSensorUsing(object obj)
    {
        if (obj is not (int obsId, int boardId, int sensorId, bool isUsing)) return;

        if (this.obsId != obsId) return;

        //List<ObsMonitoringItem> tItems = new[] { toxinItems, chemicalItems, qualityItems }[boardId-1];
        //new[] { toxinItems, chemicalItems, qualityItems }[boardId-1][sensorId-1]?.gameObject.SetActive(isUsing);

        List<ObsMonitoringItem> tItems = null;
        switch (boardId)
        {
            case 1: tItems = toxinItems; break;
            case 2: tItems = chemicalItems; break;
            case 3: tItems = qualityItems; break;
        }
        if (tItems is null) return;

        ObsMonitoringItem tItem = null;
        tItem = tItems[sensorId - 1];
        if (tItem is null) return;

        //아이템 활성화
        tItem.gameObject.SetActive(isUsing);
        //레이아웃 갱신
        RectTransform rt = tItem.transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }


    private void OnChangeSensorStatus(object obj) => allItems.ForEach(item => item.ResetToxinStatus());
    
    private void OnChangeTrendLine(object obj) => allItems.ForEach(item => item.UpdateTrendLine());
    
    private void OnChangeSensorList(object obj) => ApplySensorList(modelProvider.GetToxins());
    

    private void OnClickSetting()
    {
        UiManager.Instance.Invoke(UiEventType.PopupSetting, obsId);
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

    void ApplySensorListBoard(List<ToxinData> toxinsInBoard, List<ObsMonitoringItem> items)
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
                ObsMonitoringItem newItem = obj.GetComponent<ObsMonitoringItem>();
                items.Add(newItem);
            }
        }

        //전체 아이템 순회
        for (int i = 0; i < items.Count; i++) 
        {
            ObsMonitoringItem item = items[i];
            ToxinData toxin = toxinsInBoard[i];

            //빈자리라면 비활성화
            if (i + 1 > toxinsInBoard.Count)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.SetToxinData(obsId, toxin);
        }

        //초기화
        RectTransform rt = items[0].transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void SetTitleStatus(ToxinStatus status)
    {
        imgSingalLamp.color = statusColorDic[status];
        switch (status)
        {
            case ToxinStatus.Green: lblStatus.text = "정 상"; break;
            case ToxinStatus.Yellow: lblStatus.text = "경 계"; break;
            case ToxinStatus.Red: lblStatus.text = "경 보"; break;
            case ToxinStatus.Purple: lblStatus.text = "설비 이상"; break;
        }

    }

    void SetAnimation(Vector3 toPos, float duration)
    {
        Vector3 fromPos = GetComponent<RectTransform>().position;
        DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() => {
            GetComponent<RectTransform>().position = fromPos;
        });

    }
}