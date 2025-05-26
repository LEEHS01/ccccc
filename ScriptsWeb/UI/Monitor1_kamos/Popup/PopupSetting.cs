using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

internal class PopupSetting : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    #region[UI Componenets]
    //PopupSetting
    Button btnClose;
    GameObject pnlTabObs, pnlTabSystem;
    Button btnTabObs, btnTabSystem;

    //TabObs
    TMP_Dropdown ddlArea, ddlObs;
    Toggle tglBoardToxin, tglBoardQuality, tglBoardChemical;
    TMP_InputField txtCctvEquipment, txtCctvOutdoor, txtSearchQuality, txtSearchChemical;
    List<PopupSettingItem> chlUsingQuality, chlUsingChemical;

    //TabSystem
    Slider sldAlarmPopup;
    TMP_InputField txtDbAddress;
    #endregion [UI Components]
    Image imgSliderHandle;

    #region [Variables]
    //선택한 지역 값
    int areaId, obsId;

    //팝업 시 사용할 기본 위치
    Vector3 defaultPos;
    #endregion [Variables]

    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    static PopupSetting()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#FFF600"},
            { ToxinStatus.Yellow,   "#FF0000"},
            { ToxinStatus.Red,      "#6C00E2"},
            { ToxinStatus.Purple,   "#C6C6C6"},
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                statusColorDic[pair.Key] = color;
    }

    private void Start()
    {
        //PopupSetting 구성요소
        { 
            Transform conTabButtons = transform.Find("conTabButtons");
            {
                btnTabObs = conTabButtons.Find("btnTabObs").GetComponent<Button>();
                btnTabObs.onClick.AddListener(() => OnOpenTab(pnlTabObs));
                btnTabSystem = conTabButtons.Find("btnTabSystem").GetComponent<Button>();
                btnTabSystem.onClick.AddListener(() => OnOpenTab(pnlTabSystem));
                //btnTabCctv = conTabButtons.Find("btnCctv").GetComponent<Button>();
                //btnTabCctv.onClick.AddListener(() => OnOpenTab(pnlTabCctv));
            }

            Transform conTabPanels = transform.Find("conTabPanels");
            {
                pnlTabObs = conTabPanels.Find("pnlTabObs").gameObject;
                pnlTabSystem = conTabPanels.Find("pnlTabSystem").gameObject;
                //pnlTabCctv = conTabPanels.Find("tabCctv").gameObject;
            }

            btnClose = transform.Find("btnClosePopup").GetComponent<Button>();
            btnClose.onClick.AddListener(OnCloseSetting);
        }

        //tabObs 구성요소
        {
            Transform pnlSelectObs = pnlTabObs.transform.Find("pnlSelectObs");
            {
                ddlArea = pnlSelectObs.Find("ddlArea").GetComponent<TMP_Dropdown>();
                ddlArea.onValueChanged.AddListener(OnSelectArea);
                ddlObs = pnlSelectObs.Find("ddlObs").GetComponent<TMP_Dropdown>();
                ddlObs.onValueChanged.AddListener(OnSelectObs);
            }

            Transform pnlBoardToxin = pnlTabObs.transform.Find("pnlBoardToxin");
            {
                tglBoardToxin = pnlBoardToxin.Find("tglBoardFixing").GetComponent<Toggle>();
                tglBoardToxin.onValueChanged.AddListener(isFixing => OnToggleBoard(1, !isFixing));
            }

            Transform pnlBoardQuality = pnlTabObs.transform.Find("pnlBoardQuality");
            {
                tglBoardQuality = pnlBoardQuality.Find("tglBoardFixing").GetComponent<Toggle>();
                tglBoardQuality.onValueChanged.AddListener(isFixing => OnToggleBoard(3, !isFixing));

                txtSearchQuality = pnlBoardQuality.Find("txtSearchSensor").GetComponent<TMP_InputField>();
                txtSearchQuality.onValueChanged.AddListener(OnChangeSearchSensor);

                chlUsingQuality = pnlBoardQuality.GetComponentsInChildren<PopupSettingItem>().ToList();
                chlUsingQuality.ForEach(item => item.SetItem(obsId, 3, -1, "불러오는 중...", true));
            }

            Transform pnlBoardChemical = pnlTabObs.transform.Find("pnlBoardChemical");
            {
                tglBoardChemical = pnlBoardChemical.Find("tglBoardFixing").GetComponent<Toggle>();
                tglBoardChemical.onValueChanged.AddListener(isFixing => OnToggleBoard(2, !isFixing));

                txtSearchChemical = pnlBoardChemical.Find("txtSearchSensor").GetComponent<TMP_InputField>();
                txtSearchChemical.onValueChanged.AddListener(OnChangeSearchSensor);

                chlUsingChemical = pnlBoardChemical.GetComponentsInChildren<PopupSettingItem>().ToList();
                chlUsingChemical.ForEach(item => item.SetItem(obsId, 2, -1, "불러오는 중...", true));
            }

            Transform pnlCctvUrl = pnlTabObs.transform.Find("pnlCctvUrl");
            {
                txtCctvEquipment = pnlCctvUrl.Find("txtUrlEquipment").GetComponent<TMP_InputField>();
                txtCctvEquipment.onValueChanged.AddListener(url => OnChangeCctv(CctvType.EQUIPMENT, url));
                txtCctvOutdoor = pnlCctvUrl.Find("txtUrlOutdoor").GetComponent<TMP_InputField>();
                txtCctvOutdoor.onValueChanged.AddListener(url => OnChangeCctv(CctvType.OUTDOOR, url));
            }
        }

        //tabSystem 구성요소
        {
            Transform pnlAlarmSetting = pnlTabSystem.transform.Find("pnlAlarmSetting");
            {
                sldAlarmPopup = pnlAlarmSetting.GetComponentInChildren<Slider>();
                int selectionCount = Enum.GetNames(typeof(ToxinStatus)).Length;
                sldAlarmPopup.value = (1f - (float)Option.alarmThreshold / (selectionCount - 1));
                sldAlarmPopup.onValueChanged.AddListener(OnAlarmSliderChanged);

                imgSliderHandle = sldAlarmPopup.transform.Find("Handle Slide Area").Find("Handle").GetComponent<Image>();
                imgSliderHandle.color = statusColorDic[Option.alarmThreshold];
            }

            Transform pnlDatabase = pnlTabSystem.transform.Find("pnlDatabase");
            {
                txtDbAddress = pnlDatabase.GetComponentInChildren<TMP_InputField>();
                txtDbAddress.text = Option.url;
                txtDbAddress.onValueChanged.AddListener(OnChangeDbAddress);
            }
        }


        UiManager.Instance.Register(UiEventType.PopupSetting, OnPopupSetting);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.ChangeSettingSensorList, OnChangeSettingSensorList);
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        defaultPos = transform.position;

        //초기화 진행
        OnOpenTab(this.pnlTabObs);
        gameObject.SetActive(false);

        //Debug!
        UiManager.Instance.Register(UiEventType.CommitSensorUsing, tuple => Debug.LogWarning("CommitSensorUsing occured : " + tuple));
        UiManager.Instance.Register(UiEventType.CommitBoardFixing, tuple => Debug.LogWarning("CommitBoardFixing occured : " + tuple));
        UiManager.Instance.Register(UiEventType.CommitCctvUrl, tuple => Debug.LogWarning("CommitCctvUrl occured : " + tuple));
    }

    #region [Basic Function]
    private void OnInitiate(object obj)
    {
        LoadAreaList();
        LoadObs(1);
    }
    private void OnPopupSetting(object obj)
    {
        if (obj is not int obsId) return;

        bool isFromObs = (obsId >= 1);

        //ObsMornitoring을 통해 켜질 때, 해당 관측소의 탭만을 제공
        if (isFromObs) 
        {
            OnOpenTab(pnlTabObs);
            OnNavigateObs(obsId);
        }
        btnTabSystem.gameObject.SetActive(!isFromObs);

        transform.position = defaultPos;
        gameObject.SetActive(true);
    }
    private void OnNavigateObs(object obj) 
    {
        if (obj is not int obsId) return;

        this.obsId = obsId;
        ObsData obs = modelProvider.GetObs(obsId);

        TMP_Dropdown.OptionData areaDdlOpt = ddlArea.options.Find(opt => opt.text == obs.areaName);
        int areaDdlIdx = ddlArea.options.IndexOf(areaDdlOpt);
        ddlArea.value = areaDdlIdx;

        TMP_Dropdown.OptionData obsDdlOpt = ddlObs.options.Find(opt => opt.text == obs.obsName);
        int obsDdlIdx = ddlObs.options.IndexOf(obsDdlOpt);
        ddlObs.value = obsDdlIdx;
    }
    private void OnChangeSettingSensorList(object obj)
    {
        List<ToxinData> toxins = modelProvider.GetToxinsSetting();
        
        //화학물질 센서들
        List<ToxinData> toxinsChemical = toxins.Where(toxin => toxin.boardid == 2).ToList();
        {
            for (int i = 0; i < toxinsChemical.Count; i++)
            {
                int sensorId = i + 1;
                ToxinData toxin = toxinsChemical.Find(item => item.hnsid == sensorId);
                PopupSettingItem item = chlUsingChemical[i];
                item.SetItem(obsId, 2, sensorId, toxin.hnsName, toxin.on);
            }
            OnChangeSearchSensor("");
        }
        
        //수질 센서들
        List<ToxinData> toxinsQuality = toxins.Where(toxin => toxin.boardid == 3).ToList();
        {
            for (int i = 0; i < toxinsQuality.Count; i++)
            {
                int sensorId = i + 1;
                ToxinData toxin = toxinsQuality.Find(item => item.hnsid == sensorId);
                PopupSettingItem item = chlUsingQuality[i];
                item.SetItem(obsId, 3, sensorId, toxin.hnsName, toxin.on);
            }
            chlUsingQuality.ForEach(item => item.gameObject.SetActive(item.isValid));
        }

        ToxinData toxinBoard = toxins.Find(toxin => toxin.boardid == 1);
        tglBoardToxin.SetIsOnWithoutNotify(!toxinBoard.fix);

        ToxinData chemiBoard = toxins.Find(toxin => toxin.boardid == 2);
        tglBoardChemical.SetIsOnWithoutNotify(!chemiBoard.fix);

        ToxinData qualityBoard = toxins.Find(toxin => toxin.boardid == 3);
        tglBoardQuality.SetIsOnWithoutNotify(!qualityBoard.fix);

        //CCTV URL
        ObsData obs = modelProvider.GetObs(obsId);
        txtCctvEquipment.SetTextWithoutNotify(obs.src_video1);
        txtCctvOutdoor.SetTextWithoutNotify(obs.src_video2);
    }
    
    private void OnCloseSetting()
    {
        gameObject.SetActive(false);
    }
    private void OnOpenTab(GameObject targetTab)
    {
        Sprite sprTabOn = Resources.Load<Sprite>("Image/UI/Btn_Search_p");
        Sprite sprTabOff = Resources.Load<Sprite>("Image/UI/Btn_Search_n");

        btnTabObs.GetComponentInChildren<Image>().sprite = pnlTabObs == targetTab ? sprTabOn : sprTabOff;
        btnTabSystem.GetComponentInChildren<Image>().sprite = pnlTabSystem == targetTab ? sprTabOn : sprTabOff;
        //btnTabCctv  .GetComponentInChildren<Image>().sprite = pnlTabCctv == targetTab? sprTabOn : sprTabOff;
        
        Color colorGray;
        if (!ColorUtility.TryParseHtmlString("#99B1CB", out colorGray)) throw new Exception("OnOpenTab - Parsing Color Failed!");

        btnTabObs.GetComponentInChildren<TMP_Text>().color = pnlTabObs == targetTab ? Color.white : colorGray;
        btnTabSystem.GetComponentInChildren<TMP_Text>().color = pnlTabSystem == targetTab ? Color.white : colorGray;

        pnlTabObs.SetActive(pnlTabObs == targetTab);
        pnlTabSystem.SetActive(pnlTabSystem == targetTab);
        //pnlTabCctv.SetActive(pnlTabCctv == targetTab);
    }


    #endregion [Basic Function]

    #region [DropdownList]
    private void LoadAreaList()
    {
        //지역 정보 수신
        List<AreaData> areas = modelProvider.GetAreas();

        //ddl에 삽입
        ddlArea.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownItems = new();
        areas.ForEach(area => dropdownItems.Add(new(area.areaName)));
        ddlArea.AddOptions(dropdownItems);

        //기본 지역 설정
        OnSelectArea(0);
    }
    private void OnSelectArea(int idx)
    {
        areaId = idx + 1;
        LoadObs(areaId);
        ddlObs.value = 0;
    }
    private void LoadObs(int areaId)
    {
        //areaId를 통해 관측소 정보 수신
        List<ObsData> obss = modelProvider.GetObssByAreaId(areaId);

        //ddl에 삽입
        ddlObs.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownItems = new();
        obss.ForEach(obs => dropdownItems.Add(new(obs.obsName)));
        ddlObs.AddOptions(dropdownItems);

        OnSelectObs(0);
    }
    private void OnSelectObs(int idx)
    {
        ObsData obs =  modelProvider.GetObss().Find(obs => obs.obsName == ddlObs.options[idx].text);
        if (obs == null) return;

        this.obsId = obs.id;
        UiManager.Instance.Invoke(UiEventType.SelectSettingObs, obs.id);
    }
    #endregion [DropdownList]

    #region [Obs Tab]

    private void OnToggleBoard(int boardId, bool isFixing)
    {
        //Temporary Function
        UiManager.Instance.Invoke(UiEventType.CommitBoardFixing, (obsId, boardId, isFixing));
    }
    private void OnChangeSearchSensor(string text)
    {
        int visibleItemCount = 0;
        chlUsingChemical.ForEach(sensorItem => {
            bool isSearched = sensorItem.isValid && (text == "" || sensorItem.lblSensorName.text.Contains(text, StringComparison.InvariantCultureIgnoreCase));

            if (isSearched) visibleItemCount++;

            sensorItem.gameObject.SetActive(isSearched);
        });

        RectTransform itemContainer = pnlTabObs.transform.Find("pnlBoardChemical").GetComponentInChildren<VerticalLayoutGroup>().GetComponent<RectTransform>();
        itemContainer.sizeDelta = new Vector2(itemContainer.sizeDelta.x, chlUsingChemical[0].GetComponent<RectTransform>().sizeDelta.y * visibleItemCount);
    }
    private void OnChangeCctv(CctvType cctvType, string url)
    {
        //Temporary Function
        UiManager.Instance.Invoke(UiEventType.CommitCctvUrl, (obsId, cctvType, url));
    }


    #endregion [Obs Tab]

    #region [System Tab]
    private void OnAlarmSliderChanged(float value)
    {
        int selectionCount = Enum.GetNames(typeof(ToxinStatus)).Length;
        int choosenIdx = Mathf.RoundToInt((1 - value) * (selectionCount - 1));

        float normalizedSliderValue = (float)choosenIdx / (float)(selectionCount - 1);
        sldAlarmPopup.SetValueWithoutNotify(1f - normalizedSliderValue);
        imgSliderHandle.color = statusColorDic[(ToxinStatus)choosenIdx];

        PlayerPrefs.SetInt("alarmThreshold", choosenIdx);
        Option.alarmThreshold = (ToxinStatus)choosenIdx;

        UiManager.Instance.Invoke(UiEventType.CommitPopupAlarmCondition, (ToxinStatus)choosenIdx);
        //Debug.LogError($"new alarmThreshold : {choosenIdx}");
        //Debug.LogError($"new alarmThreshold : {PlayerPrefs.GetInt("alarmThreshold", -1)}");
    }
    private void OnChangeDbAddress(string dbAddress) 
    {
        PlayerPrefs.SetString("dbAddress", dbAddress);
        Option.url = dbAddress;
    }

    #endregion [System Tab]



}