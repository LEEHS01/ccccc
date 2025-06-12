using DG.Tweening;
using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    Image imgBg;
    TMP_Text lblTitle;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        imgBg = transform.GetComponent<Image>();
        lblTitle = transform.Find("lblTitle").GetComponent<TMP_Text>();
    }

    private void OnInitiate(object obj)
    {
        this.lblTitle.DOColor(new(0, 0, 0, 0), 1).SetDelay(1);
        this.imgBg.DOColor(new(0, 0, 0, 0), 1).SetDelay(1).OnComplete(()=>this.gameObject.SetActive(false));
    }
}
