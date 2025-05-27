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
        txbPassword = transform.Find("login").Find("InputField").GetComponent<TMP_InputField>();   
        btnConfirm = transform.Find("login").Find("btnLogin").GetComponent<Button>();

        UiManager.Instance.Register(UiEventType.ResponseVerification, OnVerificationResponse);
        btnConfirm.onClick.AddListener(OnClick);
    }

    private void OnVerificationResponse(object obj)
    {
        Debug.Log("OnVerificationResponse start");
        if (obj is not (bool isVerified, string verificationKey)) return;
        Debug.Log("OnVerificationResponse type good");

        if (isVerified)
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
        else { }

        Debug.Log("OnVerificationResponse is " + isVerified.ToString());
    }

    public void OnClick()
    {
        Debug.Log("OnClick RequestVerification");
        UiManager.Instance.Invoke(UiEventType.RequestVerification, txbPassword.text);
    }
}