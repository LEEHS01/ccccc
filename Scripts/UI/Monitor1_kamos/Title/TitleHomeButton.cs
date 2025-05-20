using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

internal class TitleHomeButton : MonoBehaviour
{
    Button btnNavigateHome;

    private void Start()
    {
        btnNavigateHome = GetComponentInChildren<Button>();
        btnNavigateHome.onClick.AddListener(OnClick);

        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);

        gameObject.SetActive(false);
    }

    private void OnNavigateObs(object obj)
    {
        gameObject.SetActive(true);
    }
    private void OnNavigateHome(object obj)
    {
        gameObject.SetActive(false);
    }
    private void OnNavigateArea(object obj)
    {
        gameObject.SetActive(true);
    }


    private void OnClick()
    {
        Debug.Log($"HomeButton : Clicked!");
        UiManager.Instance.Invoke(UiEventType.NavigateHome);
    }
}
