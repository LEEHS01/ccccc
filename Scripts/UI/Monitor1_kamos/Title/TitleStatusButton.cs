using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class TitleStatusButton : MonoBehaviour
{
    internal ToxinStatus status;

    Button btnStatus;
    Image imgSignalLamp;
    TMP_Text lblText;

    static Dictionary<ToxinStatus, Color> statusColorDic = new();
    //static Dictionary<AreaData.AreaType, Sprite> areaSpriteDic = new();
    static TitleStatusButton()
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
        btnStatus = GetComponent<Button>();
        lblText = GetComponentInChildren<TMP_Text>();
        imgSignalLamp = GetComponentInChildren<Image>();

        btnStatus.onClick.AddListener(() => Debug.Log("TODO"));
    }
    private void OnValidate()
    {
        lblText = GetComponentInChildren<TMP_Text>();
        imgSignalLamp = transform.Find("SignalLamp_Green").GetComponent<Image>();

        switch (status)
        {
            case ToxinStatus.Green:
                lblText.text = "정상";
                break;
            case ToxinStatus.Yellow:
                lblText.text = "경계";
                break;
            case ToxinStatus.Red:
                lblText.text = "경고";
                break;
            case ToxinStatus.Purple:
                lblText.text = "설비이상";
                break;
        }

        imgSignalLamp.color = statusColorDic[status];
    }


}
