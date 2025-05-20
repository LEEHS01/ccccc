using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


internal class UiBackground : MonoBehaviour
{
    Image imgBackground;

    private void Start()
    {
        imgBackground = GetComponent<Image>();

        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
    }

    private void OnNavigateObs(object obj)
    {
        if (imgBackground == null) throw new Exception("UiBackground - OnNavigateObs() can't find imgBackground");

        imgBackground.color = new(1f, 1f, 1f, 0f);
    }
    private void OnNavigateHome(object obj)
    {
        if (imgBackground == null) throw new Exception("UiBackground - OnNavigateHome() can't find imgBackground");

        imgBackground.color = Color.white;
    }
    private void OnNavigateArea(object obj)
    {
        if (imgBackground == null) throw new Exception("UiBackground - OnNavigateArea() can't find imgBackground");

        imgBackground.color = Color.white;
    }


}
