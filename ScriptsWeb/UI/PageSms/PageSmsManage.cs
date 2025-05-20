using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsManage : MonoBehaviour
{
    HorizontalLayoutGroup itemContainer;

    Button btnUnregister, btnRegister;

    void Start() 
    {
        btnUnregister.onClick.AddListener(OnClickUnregister);
        btnRegister.onClick.AddListener(OnClickRegister);

        LoadServiceList();
    }

    void LoadServiceList() 
    {
        //TODO
    }

    public void OnClickUnregister() => UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsUnregister));
    public void OnClickRegister() => UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsRegister));
}