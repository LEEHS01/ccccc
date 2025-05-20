using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal class TitleXrayButton : MonoBehaviour
{
    internal enum XrayType 
    {
        Equipment,  //장비
        Structure,  //건물 
    }
    internal XrayType xrayType;

    Button btnXray;
    TMP_Text lblText;

    private void Start()
    {
        btnXray = GetComponentInChildren<Button>();
        btnXray.onClick.AddListener(OnClick);

        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);

        this.gameObject.SetActive(false);
    }
    private void OnValidate()
    {
        lblText = GetComponentInChildren<TMP_Text>();
        lblText.text = xrayType == XrayType.Structure ? "건물 X-Ray" : "장비 X-Ray";
    }

    void OnClick() 
    {
        GameObject tObj = GetXrayTarget(xrayType);
        tObj.SetActive(!tObj.activeSelf);
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

        GameObject tObj = GetXrayTarget(xrayType);
        tObj.SetActive(true);
    }


    GameObject GetXrayTarget(XrayType xrayType)
    {
        try
        {
            //해당 코드는 대상 3D 모델에 직접 접근해 SetActive를 호출하는 과정임.
            //하지만 UiManager의 이벤트 호출로 이를 제어하는게 일관적인 처리로 기대됨.
            //때문에 후에UiManager에 ToggleXray같은 이벤트를 추가하는 것이 좋아보임.

            Scene currentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = currentScene.GetRootGameObjects();
            GameObject area3D = rootObjects.ToList().Find(go => go.name == "Area_3D");
            Transform observatory = area3D.transform.Find("Observatory");

            if (xrayType == XrayType.Equipment)
            {
                return observatory.Find("Equipments").gameObject;
            }
            else if (xrayType == XrayType.Structure)
            {
                return observatory.Find("OuterWall").gameObject;
            }

        }
        catch (Exception ex) 
        {
            Debug.LogError("TitleXrayButton - GetXrayTarget() - error occured!");
            Debug.LogException(ex);
            
        }

        return null;
    }



}
