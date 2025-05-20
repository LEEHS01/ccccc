using Onthesys.ExeBuild;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

internal class ObsSensorStepAnimator : MonoBehaviour
{
    Dictionary<int, List<string>> stepSensorsDic;
    Coroutine coroutine = null;
    Transform stepIndicatorParent;

    private void Awake() 
    {
        stepSensorsDic = new()
        {
            { 1, new(){
                "Sensor_A",
                "Sensor_B",
                "Sensor_C",
                "Sensor_D",
                "Sensor_E",
                "Sensor_F",
                "Sensor_G",
                "Sensor_H",
                "Sensor_I",
                "Sensor_J",
                "Sensor_K",
            } },
            { 2, new(){
                "Sensor_I",
                "Sensor_C",
                "Sensor_J",
                "Sensor_E",
                "Sensor_F",
            } },
            { 3, new(){
                "Sensor_J",
                "Sensor_D",
                "Sensor_K",
            } },
            { 4, new(){
                "Sensor_A",
                "Sensor_B",
                "Sensor_D",
                "Sensor_K",
            } },
            { 5, new() },
        };
    }
    private void Start()
    {
        stepIndicatorParent = transform.Find("Canvas");

        UiManager.Instance.Register(UiEventType.ChangeSensorStep, OnChangeSensorStep);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateHome);
    }

    private void OnNavigateHome(object obj)
    {
        SetStepIndicator(-1);
        ResetAllOutlines();

        if (coroutine is not null) StopCoroutine(coroutine);
    }

    private void OnChangeSensorStep(object obj)
    {
        if (obj is not int step) return;

        SetStepIndicator(step);
        ResetAllOutlines();

        if (coroutine is not null) StopCoroutine(coroutine);

        coroutine = StartCoroutine(AnimateOutlineWidth(step));
    }

    void ResetAllOutlines()
    {
        IEnumerable<Outline> allOutlines = transform.GetComponentsInChildren<Outline>();
        foreach(var outline in allOutlines)
        {
            outline.OutlineWidth = 0; // 아웃라인 두께를 0으로 설정하여 숨김
            outline.enabled = false; // 비활성화하여 강제 업데이트
            outline.enabled = true;  // 다시 활성화
        };
    }

    IEnumerator AnimateOutlineWidth(int stage)
    {
        float duration = 1f; // 1초 동안 애니메이션 진행
        float elapsedTime = 0f;
        float minOutlineWidth = 0f;
        float maxOutlineWidth = 20f;

        List<string> stepSensorNames = stepSensorsDic[stage];
        IEnumerable<Outline> allOutlines = transform.GetComponentsInChildren<Outline>();
        IEnumerable<Outline> stepOutlines = allOutlines.Where(outline => stepSensorNames.Contains(outline.name));

        while (true)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.PingPong(elapsedTime / duration, 1f);
            float outlineWidth = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, t);

            foreach (Outline outline in stepOutlines)
            {
                outline.OutlineWidth = outlineWidth; // OutlineWidth 값 변경
                outline.enabled = true; // 활성화하여 아웃라인 표시
                outline.OutlineMode = Outline.Mode.OutlineAll; // Outline 모드 설정 (필요한 경우)
            }

            yield return null;
        }
    }

    void SetStepIndicator(int step) 
    {
        foreach (Transform child in stepIndicatorParent)
            child.gameObject.SetActive(child.name == ("Step_0" + step));
    }

}

