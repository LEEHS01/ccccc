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
    ModelProvider modelProvider => UiManager.Instance.modelProvider;
    Transform itemContainer;
    List<ItemSmsManage> smsItems => itemContainer.GetComponentsInChildren<ItemSmsManage>().ToList();

    Button btnUnregister, btnRegister;

    void Start() 
    {
        btnUnregister = transform.Find("List").Find("btnDelete").GetComponent<Button>();
        btnUnregister.onClick.AddListener(OnClickUnregister);

        btnRegister = transform.Find("List").Find("btnInput").GetComponent<Button>();
        btnRegister.onClick.AddListener(OnClickRegister);

        itemContainer = transform.Find("List").Find("Scroll View").Find("Viewport").Find("SmsServicePanel");

        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);
        gameObject.SetActive(false);
    }

    private void OnNavigateSms(object obj)
    {
        if (obj is not Type type) return;
        
        if (type != typeof(PageSmsManage)) return;

        LoadServiceList();
    }

    void LoadServiceList() 
    {
        List<SmsServiceModel> items = modelProvider.GetSmsServices();
        items.ForEach(service => Debug.Log($"SMS Service: {service.service_id}, {service.name}, {service.phone}"));

        while (smsItems.Count < items.Count)
        {
            var item = smsItems.First().gameObject;
            var gameObject = Instantiate<GameObject>(item);

            gameObject.transform.SetParent(itemContainer, false);
        }
        while (smsItems.Count > items.Count)
        {
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

    public void OnClickUnregister() => UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsUnregister));
    public void OnClickRegister() => UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsRegister));
}