using Onthesys;
using Onthesys.ExeBuild;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class MapNationMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    internal AreaData.AreaType areaType;
    internal ToxinStatus status;
    internal string areaName;
    int areaId;

    internal Sprite nuclearSprite;
    internal Sprite oceanSprite;

    Animator animator;
    GameObject focusImage;
    Button btnNavigateArea;

    static Dictionary<ToxinStatus, Color> statusColorDic = new();
    static MapNationMarker() 
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#7AE5AC"},
            { ToxinStatus.Yellow,   "#DFE50C"},
            { ToxinStatus.Red,      "#E57A9E"},
            { ToxinStatus.Purple,   "#6C00E2"},
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString( htmlString : pair.Value, out color)) 
                statusColorDic[pair.Key] = color;
    }

    private void OnValidate()
    {
        Image imageCircle =     transform.Find("Point_location").Find("Icon_location01Circle").GetComponent<Image>();
        Image imageMain =       transform.Find("Point_location").Find("Icon_location01").GetComponent<Image>();
        Image imageIcon =       transform.Find("Point_location").Find("Icon_location01").Find("icon").GetComponent<Image>();
        TMP_Text textAreaName = transform.Find("TmpAreaName").GetComponent<TMP_Text>();

        imageCircle.color = new Color(
            statusColorDic[status].r, 
            statusColorDic[status].g, 
            statusColorDic[status].b, 
            imageCircle.color.a);
        imageMain.color = statusColorDic[status];
        imageIcon.sprite = areaType == AreaData.AreaType.Ocean? oceanSprite:nuclearSprite;// areaSpriteDic[areaType];
        textAreaName.text = areaName.Substring(0,2);
    }
    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);

        btnNavigateArea = GetComponent<Button>();
        btnNavigateArea.onClick.AddListener(OnClick);
        animator = GetComponentInChildren<Animator>();
        focusImage = transform.Find("Focus").gameObject;
        focusImage.SetActive(false);
    }

    internal void SetAreaData(int areaId, string areaName, AreaData.AreaType areaType, ToxinStatus status)
    {
        this.areaId = areaId;
        this.areaName = areaName;
        this.areaType = areaType;
        this.status = status;
        OnValidate();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        animator?.SetTrigger("Play"); // Play 트리거를 설정
        focusImage.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(StopAnimationAfterDelay(1.0f)); // 1초 후에 Stop 트리거를 설정
        focusImage.SetActive(false);

    }

    private IEnumerator StopAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator?.SetTrigger("Stop"); // Stop 트리거를 설정
    }


    private void OnNavigateHome(object obj)
    {
        animator?.SetTrigger("Stop"); // Stop 트리거를 설정
        focusImage.SetActive(false);
    }

    private void OnClick() 
    {
        Debug.Log($"MapNationMarker({areaName}) : Clicked!");

        UiManager.Instance.Invoke(UiEventType.NavigateArea, areaId);
    }
}