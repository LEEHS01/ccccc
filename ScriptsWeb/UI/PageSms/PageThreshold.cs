using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


internal class PageThreshold : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    TMP_Dropdown ddlSensor;
    TMP_InputField txbWarningValue, txbSeriousValue;
    Button btnConfirm, btnCancel;

    private void Start()
    {
        ddlSensor = transform.Find("ddlSensor").GetComponent<TMP_Dropdown>();
        txbWarningValue = transform.Find("InputWarningvalue").GetComponent<TMP_InputField>();
        txbSeriousValue = transform.Find("InputSeriousvalue").GetComponent<TMP_InputField>();

        btnCancel = transform.Find("btnClose").GetComponent<Button>();
        btnCancel.onClick.AddListener(OnClickCancel);

        btnConfirm = transform.Find("btnInput").GetComponent<Button>();
        btnConfirm.onClick.AddListener(OnClickConfirm);

        // TODO: 이벤트 등록
        // UiManager.Instance.Register(UiEventType.NavigateThreshold, OnNavigateThreshold);

        gameObject.SetActive(false);
    }

    void OnClickConfirm()
    {
        // TODO: 임계값 업데이트 로직
        Debug.Log("임계값 저장 버튼 클릭");
    }

    void OnClickCancel()
    {
        // TODO: 이전 페이지로 돌아가기
        Debug.Log("취소 버튼 클릭");
    }

}

