using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


internal class MapAreaMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    internal ToxinStatus status;
    internal string obsName;
    int obsId = -1;

    Animator animator;
    Button btnNavigateObs;

    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    //static Dictionary<AreaData.AreaType, Sprite> areaSpriteDic = new();
    static MapAreaMarker()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#7AE5AC"},
            { ToxinStatus.Yellow,   "#DFE50C"},
            { ToxinStatus.Red,      "#E57A9E"},
            { ToxinStatus.Purple,   "#6C00E2"},
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                statusColorDic[pair.Key] = color;
    }


    private void OnValidate()
    {
        Image imageCircle = transform.Find("location_Circle").GetComponent<Image>();
        Image imageLine = transform.Find("location_DottedLine").GetComponent<Image>();
        Image imageMain = transform.Find("location_Btn").GetComponent<Image>();
        TMP_Text textObsName = transform.Find("location_Btn").GetComponentInChildren<TMP_Text>();

        imageCircle.color = new Color(
            statusColorDic[status].r,
            statusColorDic[status].g,
            statusColorDic[status].b,
            imageCircle.color.a);
        imageLine.color = statusColorDic[status];
        imageMain.color = statusColorDic[status];
        textObsName.text = obsName;
    }
    private void Start()
    {
        btnNavigateObs = GetComponentInChildren<Button>();
        btnNavigateObs.onClick.AddListener(OnClick);
        animator = GetComponentInChildren<Animator>();
    }
    
    
    internal void SetObsData(int obsId, string obsName, ToxinStatus status )
    {
        this.status = status;
        this.obsId = obsId;
        this.obsName = obsName;
        OnValidate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator?.SetTrigger("Start"); // Play 트리거를 설정
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator?.SetTrigger("Stop"); // Stop 트리거를 설정
    }
    private void OnClick()
    {
        Debug.Log($"MapAreaMarker({obsName}) : Clicked!");
        UiManager.Instance.Invoke(UiEventType.NavigateObs, obsId);
    }
}
