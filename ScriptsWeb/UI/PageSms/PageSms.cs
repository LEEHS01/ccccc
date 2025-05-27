using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PageSms : MonoBehaviour 
{
    Button btnNavigateMain; 
    Dictionary<Type, GameObject> pages = new Dictionary<Type, GameObject>();

    public static string verificationKey { private set; get; }

    private void Awake()
    {
        pages.Add(typeof(PageSmsAuth), transform.Find("Panels").GetComponentInChildren<PageSmsAuth>().gameObject);
        pages.Add(typeof(PageSmsManage), transform.Find("Panels").GetComponentInChildren<PageSmsManage>().gameObject);
        pages.Add(typeof(PageSmsRegister), transform.Find("Panels").GetComponentInChildren<PageSmsRegister>().gameObject);
        pages.Add(typeof(PageSmsUnregister), transform.Find("Panels").GetComponentInChildren<PageSmsUnregister>().gameObject);
        pages.Add(typeof(PageSmsUpdate), transform.Find("Panels").GetComponentInChildren<PageSmsUpdate>().gameObject);
    }

    private void Start()
    {
        btnNavigateMain = transform.Find("Title_UI").Find("HomeBtn").GetComponent<Button>(); 
        btnNavigateMain.onClick.AddListener(() =>
        {
            UiManager.Instance.Invoke(UiEventType.NavigateMain);
        });
        UiManager.Instance.Register(UiEventType.ResponseVerification, OnVerificationResponse);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);


        gameObject.SetActive(false);
    }

    private void OnVerificationResponse(object obj)
    {
        if (obj is not (bool isVerified, string verificationKey)) return;
        
        if(isVerified)
            PageSms.verificationKey = verificationKey;
    }
    public void OnNavigateSms(object obj)
    {
        if (obj is null) 
        {
            gameObject.SetActive(true);

            foreach (var item in pages)
                item.Value.SetActive(item.Key == typeof(PageSmsAuth));
        }

        if (obj is not Type pageType) return;

        foreach (var item in pages)
            item.Value.SetActive(item.Key == pageType);
    }
    public void OnNavigateMain(object obj)
    {
        gameObject.SetActive(false);
    }

}