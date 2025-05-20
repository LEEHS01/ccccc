using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsAuth : MonoBehaviour 
{
    TMP_InputField txbPassword;
    Button btnConfirm;



    private void Start()
    {
        UiManager.Instance.Register(UiEventType.ResponseVerification, OnVerificationResponse);
        btnConfirm.onClick.AddListener(OnClick);
    }

    private void OnVerificationResponse(object obj)
    {
        if (obj is not (bool isVerified, string verificationKey)) return;

        if(isVerified)
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
        else { }

    }

    public void OnClick() 
    {
        UiManager.Instance.Invoke(UiEventType.RequestVerification, txbPassword.text);
    }
}