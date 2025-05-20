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
    Toggle tglTimespan;
    Button btnNavigateSms;

    private void Start()
    {
        tglTimespan.onValueChanged.AddListener(OnToggleTimespan);
        btnNavigateSms.onClick.AddListener(OnClickSms);
        UiManager.Instance.Register(UiEventType.NavigateMain, OnNavigateMain);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);
    }

    private void OnNavigateSms(object obj)
    {
        gameObject.SetActive(false);
    }

    private void OnNavigateMain(object obj)
    {
        gameObject.SetActive(true);
    }

    void OnToggleTimespan(bool isWeek) => UiManager.Instance.Invoke(UiEventType.ChangeTimespan, isWeek);
    void OnClickSms() => UiManager.Instance.Invoke(UiEventType.NavigateSms);

}