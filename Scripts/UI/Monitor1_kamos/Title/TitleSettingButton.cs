using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

internal class TitleSettingButton : MonoBehaviour
{
    Button btnPopupSetting;

    private void Start()
    {
        btnPopupSetting = GetComponentInChildren<Button>();
        btnPopupSetting.onClick.AddListener(OnClick);
    }


    private void OnClick()
    {
        Debug.Log($"PopupSetting : Clicked!");
        UiManager.Instance.Invoke(UiEventType.PopupSetting, 0);
    }
}