using DG.Tweening;
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

internal class AreaListType : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    //Main Parameter
    internal AreaData.AreaType areaType;
    internal Sprite nuclearSprite;
    internal Sprite oceanSprite;

    //Core Components
    Image imgAreaType;
    TMP_Text lblTitle;
    List<AreaListTypeItem> items = new();


    Vector3 defaultPos;

    private void OnValidate()
    {
        imgAreaType = transform.Find("icon").GetComponent<Image>();
        imgAreaType.sprite = areaType == AreaData.AreaType.Ocean ? oceanSprite : nuclearSprite;

        lblTitle = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        lblTitle.text = areaType == AreaData.AreaType.Ocean ? "해양산업시설 방류구 및 주변 해역" : "발전소 방류구 및 주변 해역";
    }

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnInitiate);

        imgAreaType = transform.Find("icon").GetComponent<Image>();
        lblTitle = transform.Find("Text (TMP)").GetComponent<TMP_Text>();

        items = transform.Find("List_Panel").GetComponentsInChildren<AreaListTypeItem>().ToList();

        defaultPos = transform.position;
    }

    private void OnInitiate(object obj)
    {
        List<AreaData> areasInType = modelProvider.GetAreas().Where(area => area.areaType == this.areaType).ToList();

        for (int i = 0; i < items.Count; i++) 
        {
            AreaListTypeItem item = items[i];
            AreaData area = areasInType[i];
            AlarmCount alarmCount = modelProvider.GetObsStatusCountByAreaId(area.areaId);

            item.SetAreaData(area.areaId, area.areaName, alarmCount);
        }
    }

    private void OnNavigateArea(object obj)
    {
        //this.gameObject.SetActive(false);
        SetAnimation(defaultPos + new Vector3(800, 0f), 1f);
    }
    private void OnNavigateHome(object obj)
    {
        //this.gameObject.SetActive(true);
        SetAnimation(defaultPos , 1f);
    }
    private void OnNavigateObs(object obj)
    {
        //this.gameObject.SetActive(false);
        SetAnimation(defaultPos + new Vector3(800, 0f), 1f);
    }


    void SetAnimation(Vector3 toPos, float duration)
    {
        Vector3 fromPos = GetComponent<RectTransform>().position;
        DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() => {
            GetComponent<RectTransform>().position = fromPos;
        });

    }

}
