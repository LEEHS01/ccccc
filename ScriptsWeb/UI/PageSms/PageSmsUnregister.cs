using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsUnregister : MonoBehaviour
{
    HorizontalLayoutGroup itemContainer;

    Button btnConfirm, btnCancel;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.ResponseSmsUnregister, OnResponseSmsUnregister);

        btnConfirm.onClick.AddListener(OnClickConfirm);
        btnCancel.onClick.AddListener(OnClickCancel);
    }

    private void OnResponseSmsUnregister(object obj)
    {
        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            //성공알림 TODO
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
        }
        else
        {
            //실패알림 TODO
        }
    }

    StatusType GetTypeFromDropdown()
    {
        //TODO
        return StatusType.WARNING;
    }
    void OnClickConfirm()
    {
        List<int> serviceIdList = itemContainer.GetComponentsInChildren<ItemSmsDelete>().Where(item => item.isChecked).Select(item => item.data.service_id).ToList();

        UiManager.Instance.Invoke(UiEventType.RequestSmsUnregister, serviceIdList);
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

}