using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using Onthesys.ExeBuild;

internal class MapArea : MonoBehaviour 
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    /// <summary>
    /// 선택한 지역 ID, NavigateArea이벤트에 따라 변화함.
    /// </summary>
    internal int areaId = -1;

    List<List<Vector3>> obssPos;
    List<string> areaTextureRoots;

    List<MapAreaMarker> areaMarkers;
    Image imgArea;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, obj => OnNavigateArea(areaId));


        areaMarkers = transform.Find("MarkerList").GetComponentsInChildren<MapAreaMarker>(true).ToList();
        imgArea = transform.Find("MapImage").GetComponent<Image>();

        string prefabPath = "Prefab/MapAreaLoadout";
        obssPos = GetObssPos(prefabPath);
        if (obssPos == null)
            throw new Exception("MapArea - GetObssPos() returned null!");

        //추후, 로컬 저장소등을 통해 프로그램 재빌드 없이 이미지 추가가 가능하게 하는 것도?
        //물론 지역추가를 고려하게 되면 새로 추가해야할 것들이 엄청 많아지니 일단 보류
        //또한 당장에 MapArea가 프리팹을 통해 맵 정보를 받아오는 상황이다
        areaTextureRoots = new() {
            "Image/AreaBackground/InCheon",
            "Image/AreaBackground/PyeongTaek",
            "Image/AreaBackground/YeoSu",
            "Image/AreaBackground/BuSan", 
            "Image/AreaBackground/UlSan", 

            "Image/AreaBackground/BoRyeong", 
            "Image/AreaBackground/YeongGwang",
            "Image/AreaBackground/SaCheon",
            "Image/AreaBackground/GoRi",
            "Image/AreaBackground/DongHae", //세번쨰 하하 두번째 좌 첫번째 좌
        };

        gameObject.SetActive(false);
    }


    private void OnNavigateArea(object obj)
    {
        if (obj is int newAreaId) 
        {
            if (newAreaId < 1) return;

            areaId = newAreaId;
            SetByAreaId(areaId);
            this.gameObject.SetActive(true);
        }
    }
    private void OnNavigateHome(object obj)
    {
        areaId = -1;
        this.gameObject.SetActive(false);
    }
    private void OnNavigateObs(object obj)
    {
        areaId = -1;
        this.gameObject.SetActive(false);
    }

    bool SetByAreaId(int areaId)
    {
        Debug.Log("SetByAreaId");
        int areaIdx = areaId - 1;
        try
        {
            //지역 배경 설정
            //Texture2D mapTex = Resources.Load<Texture2D>(areaTextureRoots[areaIdx]);
            //Debug.Log("mapTex : " + mapTex);
            //Sprite loadedSprite = Sprite.Create(mapTex,new Rect(), Vector2.one * 0.5f);
            imgArea.sprite = Resources.Load<Sprite>(areaTextureRoots[areaIdx]);
            //Debug.Log("loadedSprite : " + imgArea.sprite);

            var obssInArea = modelProvider.GetObssByAreaId(areaId);

            //예외
            if (obssInArea.Count != areaMarkers.Count)
                throw new Exception("MapArea - SetByAreaId : 예상 범위 밖의 값이 입력되었습니다. 표시하기 위한 관측소 수가 지역 내 관측소 수와 일치하지 않습니다.");

            //지역 마커 설정
            for (int i = 0; i < areaMarkers.Count; i++)
            {
                var areaMarker = areaMarkers[i];
                var obs = obssInArea[i];
                var obsStatus = modelProvider.GetObsStatus(obs.id);

                areaMarker.SetObsData(obs.id, obs.obsName, obsStatus);
                areaMarker.transform.position = this.transform.position + obssPos[areaIdx][i];
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"MapArea - SetByAreaId() Failed! areaIdx : {areaId}");
            Debug.LogException(ex);
            return false;
        }
    }
    List<List<Vector3>> GetObssPos(string prefabPath)
    {
        //GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        List<List<Vector3>> list = new List<List<Vector3>>();

        foreach (Transform child in prefab.transform)
        {
            List<Vector3> areaNode = new();
            //Debug.Log("Area : " + child.name);
            foreach (Transform grandChild in child)
            {
                //Debug.Log("obs : " + grandChild.position.ToString());
                areaNode.Add(grandChild.position);
            }
            list.Add(areaNode);
        }

        return list;
    }
}
