using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


internal class PopupError : MonoBehaviour
{
    Vector3 deafultPos;

    TMP_Text lblTitle, lblMessage;
    Button btnClose; 

    private void Awake()
    {

        Transform titleLine = transform.Find("TitleLine");
        lblTitle = titleLine.Find("lblTitle").GetComponent<TMP_Text>();
        btnClose = titleLine.Find("btnClose").GetComponent<Button>();

        lblMessage = transform.Find("lblSummary").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        deafultPos = transform.position;
        UiManager.Instance.Register(UiEventType.PopupError, OnPopupError);
        UiManager.Instance.Register(UiEventType.PopupError, ex=>Debug.LogError(ex));

        btnClose.onClick.AddListener(OnCloseError);
        gameObject.SetActive(false);
    }

    void OnPopupError(object obj)
    {
        Exception showEx = null;

        if (obj is TargetInvocationException invocationEx)
            showEx = invocationEx.InnerException;

        if (obj is Exception ex)
            showEx = ex;

        if (showEx == null) return;

        transform.position = deafultPos;
        gameObject.SetActive(true);

        lblTitle.text = "오류 발생";
        lblMessage.text = $"{showEx.Message}\n\n{showEx.GetType()}";
    }

    void OnCloseError() 
    {
        gameObject.SetActive(false);
    }


}