using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalAlert : MonoBehaviour
{
    Button btnConfirm, btnClose;
    TMP_Text lblContext, lblTitle;
    void Start()
    {
        btnConfirm = transform.Find("btnConfirm").GetComponent<Button>();
        btnClose = transform.Find("pnlTitle").Find("btnClose").GetComponent<Button>();
        lblContext = transform.Find("lblContext").GetComponent<TMP_Text>();
        lblTitle = transform.Find("pnlTitle").Find("lblTitle").GetComponent<TMP_Text>();
        btnConfirm.onClick.AddListener(() => gameObject.SetActive(false));
        btnClose.onClick.AddListener(() => gameObject.SetActive(false));

        UiManager.Instance.Register(UiEventType.PopupError, OnPopupError);

        gameObject.SetActive(false);
    }

    private void OnPopupError(object obj)
    {
        if (obj is not (string title, string context)) return;

        gameObject.SetActive(true);

        lblTitle.text = title;
        lblContext.text = context;
    }
}

