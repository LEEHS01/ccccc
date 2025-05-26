using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class GraphicHider : MonoBehaviour 
{
    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);

        this.gameObject.SetActive(false);
    }

    private void OnNavigateArea(object obj)
    {
        this.gameObject.SetActive(false);
    }
    private void OnNavigateHome(object obj)
    {
        this.gameObject.SetActive(false);
    }
    private void OnNavigateObs(object obj)
    {
        this.gameObject.SetActive(true);
    }
}