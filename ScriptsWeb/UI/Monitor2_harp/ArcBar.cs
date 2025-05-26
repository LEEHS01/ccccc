using DG.Tweening;
using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;


internal class ArcBar : MonoBehaviour
{
    TMP_Text txtProgress;
    TMP_Text txtName;
    TMP_Text txtTotal;
    TMP_Text txtMin;
    TMP_Text txtMax;
    TMP_Text txtCurrent;
    Image arc;
    ToxinData data;

    Button btnSelectToxin;

    private void Start()
    {

        Transform dashboard = transform.Find("Dashboard");
        arc = dashboard.Find("Progress").GetComponent<Image>();
        txtProgress = dashboard.Find("Progress_Value").GetComponent<TMP_Text>();
        txtMin = dashboard.Find("Min").GetComponent<TMP_Text>();
        txtMax = dashboard.Find("Max").GetComponent<TMP_Text>();
        txtName = transform.Find("Text_DataItem").GetComponent<TMP_Text>();
        txtTotal = transform.Find("Text_DataUnit").GetComponent<TMP_Text>();
        txtCurrent= transform.Find("Text_DataValue").GetComponent<TMP_Text>();
            

        btnSelectToxin = GetComponent<Button>();
        btnSelectToxin.onClick.AddListener(OnSelectToxin);

        txtName.text = "";
        txtCurrent.text = "";
        txtProgress.text = "0";
        txtTotal.text = "";
    }


    internal void SetValue(ToxinData toxin)
    {
        gameObject.SetActive(toxin.on);
        data = toxin;

        txtName.text = toxin.hnsName;
        txtTotal.text = "/" + toxin.warning.ToString();
        //txtCurrent.text = 0;
        txtMin.text = "0";
        txtMax.text = toxin.warning.ToString();


    }

    // 아크 이미지 fill 값 설정 메서드 (기존과 동일)
    internal void SetAmount()
    {
        txtCurrent.text =  data.GetLastValue().ToString();
        txtProgress.text = data.GetLastValue().ToString();
        gameObject.SetActive(data.on);

        arc.DOFillAmount((float)(data.GetLastValuePercent()), 1);
    }

    // 센서 선택 이벤트 - UiManager로 전달
    internal void OnSelectToxin()
    {
        UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (data.boardid, data.hnsid));
    }
}

