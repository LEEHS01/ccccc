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
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    Transform itemContainer;
    List<ItemSmsDelete> smsItems => itemContainer.GetComponentsInChildren<ItemSmsDelete>().ToList();

    Button btnConfirm, btnCancel;

    private void Start()
    {
        btnCancel = transform.Find("btnClose").GetComponent<Button>();
        btnCancel.onClick.AddListener(OnClickCancel);

        btnConfirm = transform.Find("btnDelete").GetComponent<Button>();
        btnConfirm.onClick.AddListener(OnClickConfirm);

        itemContainer = transform.Find("Scroll View").Find("Viewport").Find("SmsDeletePanel");

        UiManager.Instance.Register(UiEventType.ResponseSmsUnregister, OnResponseSmsUnregister);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);
        gameObject.SetActive(false);
    }


    private void OnNavigateSms(object obj)
    {
        if (obj is not Type type) return;

        if (type != typeof(PageSmsUnregister)) return;

        LoadServiceList();
    }


    int TRY_COUNT = 0, TRY_LIMIT = 100;
    void LoadServiceList()
    {
        List<SmsServiceModel> items = modelProvider.GetSmsServices();

        while (smsItems.Count < items.Count)
        {
            if (TRY_COUNT++ > TRY_LIMIT) break;
            var item = smsItems.First().gameObject;
            var gameObject = Instantiate<GameObject>(item);

            gameObject.transform.SetParent(itemContainer, false);
        }
        while (smsItems.Count > items.Count)
        {
            if (TRY_COUNT++ > TRY_LIMIT) break;
            var item = smsItems.Last().gameObject;
            item.transform.SetParent(null, false);
            Destroy(item);
        }
        for (int i = 0; i < items.Count; i++)
        {
            smsItems[i].gameObject.SetActive(true);
            smsItems[i].SetData(items[i]);
        }

    }

    private void OnResponseSmsUnregister(object obj)
    {
        btnConfirm.interactable = true;

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
    //이거 없애도되나?
    StatusType GetTypeFromDropdown()
    {
        //TODO
        return StatusType.WARNING;
    }
    void OnClickConfirm()
    {

        List<int> serviceIdList = smsItems.Where(item => item.isChecked).Select(item => item.data.service_id).ToList();

        if (serviceIdList.Count == 0)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, ("삭제 실패", "삭제할 항목을 선택해주세요."));
            return; 
        }

        btnConfirm.interactable = false;

        UiManager.Instance.Invoke(UiEventType.RequestSmsUnregister, serviceIdList);
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

}