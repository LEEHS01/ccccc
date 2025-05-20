using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PageSms : MonoBehaviour 
{
    Button btnNavigateMain; 
    Dictionary<Type, GameObject> pages = new Dictionary<Type, GameObject>();

    public static string verificationKey { private set; get; }

    private void Start()
    {
        pages.Add(typeof(PageSmsAuth),          GetComponentInChildren<PageSmsAuth>().gameObject);
        pages.Add(typeof(PageSmsManage),        GetComponentInChildren<PageSmsManage>().gameObject);
        pages.Add(typeof(PageSmsRegister),      GetComponentInChildren<PageSmsRegister>().gameObject);
        pages.Add(typeof(PageSmsUnregister),    GetComponentInChildren<PageSmsUnregister>().gameObject);
        pages.Add(typeof(PageSmsUpdate),        GetComponentInChildren<PageSmsUpdate>().gameObject);

        btnNavigateMain.onClick.AddListener(() =>
        {
            UiManager.Instance.Invoke(UiEventType.NavigateMain);
        });
        UiManager.Instance.Register(UiEventType.ResponseVerification, OnVerificationResponse);
    }

    private void OnVerificationResponse(object obj)
    {
        if (obj is not (bool isVerified, string verificationKey)) return;
        
        if(isVerified)
            PageSms.verificationKey = verificationKey;
    }
    public void OnNavigateSms(object obj)
    {
        if (obj is not Type pageType) return;

        gameObject.SetActive(true);

        foreach (var item in pages)
            item.Value.SetActive(item.Key == pageType);
    }
    public void OnNavigateMain(object obj)
    {
        gameObject.SetActive(false);
    }

}