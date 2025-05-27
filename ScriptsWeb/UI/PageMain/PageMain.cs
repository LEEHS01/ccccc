using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainPage : MonoBehaviour
{
    Button btnTimespan;
    Button btnNavigateSms;

    private void Start()
    {
        btnTimespan = transform.Find("CycleChangeBtn").GetComponent<Button>();
        btnTimespan.onClick.AddListener(OnClickTimespan);

        btnNavigateSms = transform.Find("SmsManagerBtn").GetComponent<Button>();
        btnNavigateSms.onClick.AddListener(OnClickSms);

        //UiManager.Instance.Register(UiEventType.NavigateMain, OnNavigateMain);
        //UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);
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
    void OnClickTimespan() => UiManager.Instance.Invoke(UiEventType.ChangeTimespan, isWeek = !isWeek);
    void OnClickSms() => UiManager.Instance.Invoke(UiEventType.NavigateSms);

}