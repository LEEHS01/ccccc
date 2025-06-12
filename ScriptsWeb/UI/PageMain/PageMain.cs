using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPage : MonoBehaviour
{
    Button btnTimespan;
    Button btnNavigateSms;
    TMP_Text lblchange;

    private void Start()
    {
        btnTimespan = transform.Find("CycleChangeBtn").GetComponent<Button>();
        btnTimespan.onClick.AddListener(OnClickTimespan);

        btnNavigateSms = transform.Find("SmsManagerBtn").GetComponent<Button>();
        btnNavigateSms.onClick.AddListener(OnClickSms);

        lblchange = transform.Find("txtchange").GetComponent<TMP_Text>();

        //UiManager.Instance.Register(UiEventType.NavigateMain, OnNavigateMain);
        //UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);

        UpdateButtonDisplay();
    }

    private void OnNavigateSms(object obj)
    {
        gameObject.SetActive(false);
    }

    private void OnNavigateMain(object obj)
    {
        gameObject.SetActive(true);
    }
    bool isWeek;

    void UpdateButtonDisplay()
    {
        if (isWeek)
        {
            // 주간 모드
            lblchange.text = "주간";
        }
        else
        {
            // 일간 모드  
            lblchange.text = "일간";
        }
    }
    void OnClickTimespan()
    {
        isWeek = !isWeek;
        UpdateButtonDisplay(); // 이 줄 추가!
        UiManager.Instance.Invoke(UiEventType.ChangeTimespan, isWeek);
    }
    //void OnClickTimespan() => UiManager.Instance.Invoke(UiEventType.ChangeTimespan, isWeek = !isWeek);
    void OnClickSms() => UiManager.Instance.Invoke(UiEventType.NavigateSms);

}