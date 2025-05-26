using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Onthesys.ExeBuild;

internal class PopupMachineInfo : MonoBehaviour
{
    Button btnClose;
    Vector3 defaultPos;
    private void Start()
    {
        UiManager.Instance.Register(UiEventType.PopupMachineInfo, OnPopupSetting);

        btnClose = transform.Find("Btn_Close").GetComponent<Button>();
        btnClose.onClick.AddListener(OnCloseSetting);

        defaultPos = transform.position;
        gameObject.SetActive(false);
    }
    private void OnPopupSetting(object obj)
    {
        transform.position = defaultPos;
        gameObject.SetActive(true);
    }
    private void OnCloseSetting()
    {
        gameObject.SetActive(false);
    }
}