using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class OutlineCtrl : MonoBehaviour
{
    [Range(1, 5)]
    internal int maxStages = 5; // 스테이지 갯수 (1에서 5까지만 설정 가능)

    [Range(1, 5)]
    internal int choiceStage = 1; // 유저가 선택할 스테이지 (기본은 1단계)

    internal List<Stage> stages = new List<Stage>(); // 각 스테이지를 저장하는 리스트

    private Coroutine currentCoroutine; // 현재 실행 중인 애니메이션 코루틴

    private int lastChoiceStage = -1; // 이전 선택된 스테이지 번호를 저장

    void Start()
    {
        EnsureStageListIntegrity();
        SetCurrentStage(choiceStage);
    }

    void OnValidate()
    {
        // choiceStage 값이 maxStages 범위를 넘지 않도록 설정
        choiceStage = Mathf.Clamp(choiceStage, 1, maxStages);
        EnsureStageListIntegrity();
    }

    void Update()
    {
        // choiceStage 값이 변경되었을 때, 애니메이션을 다시 시작
        if (lastChoiceStage != choiceStage)
        {
            SetCurrentStage(choiceStage);
            lastChoiceStage = choiceStage; // 마지막 선택된 스테이지 업데이트
        }
    }

    void EnsureStageListIntegrity()
    {
        // 현재 maxStages보다 리스트의 길이가 짧다면 추가
        while (stages.Count < maxStages)
        {
            stages.Add(new Stage() { stageNumber = stages.Count + 1, objects = new List<GameObject>() });
        }

        // 현재 maxStages보다 리스트의 길이가 길다면 잘라냄
        while (stages.Count > maxStages)
        {
            stages.RemoveAt(stages.Count - 1);
        }
    }

    internal void RegisterObjectToStage(int stage, GameObject obj)
    {
        if (stage < 1 || stage > maxStages)
        {
            Debug.LogError("Invalid stage number! Must be between 1 and " + maxStages);
            return;
        }

        Outline outlineComponent = obj.GetComponent<Outline>();
        if (outlineComponent == null)
        {
            Debug.LogError("The object does not have an Outline component.");
            return;
        }

        if (!stages[stage - 1].objects.Contains(obj))
        {
            stages[stage - 1].objects.Add(obj);
        }
    }

    internal void SetCurrentStage(int stage)
    {
        if (stage < 1 || stage > maxStages)
        {
            Debug.LogError("Invalid stage number! Must be between 1 and " + maxStages);
            return;
        }

        // 모든 오브젝트의 아웃라인을 초기화
        ResetAllOutlines();

        // 현재 선택된 스테이지의 오브젝트에 대한 애니메이션 시작
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // 선택된 스테이지가 비어있지 않다면 애니메이션 시작
        if (stages[stage - 1].objects.Count > 0)
        {
            currentCoroutine = StartCoroutine(AnimateOutlineWidth(stage));
        }
    }

    internal void ResetAllOutlines()
    {
        foreach (Stage stage in stages)
        {
            foreach (GameObject obj in stage.objects)
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineWidth = 0; // 아웃라인 두께를 0으로 설정하여 숨김
                    outline.enabled = false; // 비활성화하여 강제 업데이트
                    outline.enabled = true;  // 다시 활성화
                }
            }
        }
    }

    private IEnumerator AnimateOutlineWidth(int stage)
    {
        float duration = 1f; // 1초 동안 애니메이션 진행
        float elapsedTime = 0f;
        float minOutlineWidth = 0f;
        float maxOutlineWidth = 20f;

        List<GameObject> objectsInStage = stages[stage - 1].objects;

        while (true)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.PingPong(elapsedTime / duration, 1f);
            float outlineWidth = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, t);

            foreach (GameObject obj in objectsInStage)
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineWidth = outlineWidth; // OutlineWidth 값 변경
                    outline.enabled = true; // 활성화하여 아웃라인 표시
                    outline.OutlineMode = Outline.Mode.OutlineAll; // Outline 모드 설정 (필요한 경우)
                }
            }

            yield return null;
        }
    }

    [System.Serializable]
    internal class Stage
    {
        internal int stageNumber;
        internal List<GameObject> objects; // 각 스테이지에 등록된 오브젝트 리스트
    }
}
