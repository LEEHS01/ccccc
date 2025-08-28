using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Onthesys.WebBuild;

public class AlarmBorderVisualizer : MonoBehaviour
{

    public enum AlarmLevel
    {
        Normal,     // 정상
        Warning,    // 경계 (노란색)
        Alert       // 경보 (빨간색)
    }

    [Header("센서 설정")]
    public int sensorId = 1;                     // 1: TSS, 2: BOD, 3: Turbidity

    [Header("테두리 설정")]
    public float borderWidth = 8f;               // 테두리 두께
    public bool useInnerBorder = false;          // true: 안쪽 테두리, false: 바깥쪽 테두리

    [Header("색상 설정")]
    public Color warningColor = new Color(1f, 1f, 0f, 0.9f);    // 경계 시 노랑
    public Color alertColor = new Color(1f, 0f, 0f, 0.9f);      // 경보 시 빨강

    [Header("애니메이션 설정")]
    public bool useAnimation = true;
    public AnimationType animationType = AnimationType.Pulse;
    public float animationSpeed = 2f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1f;

    public enum AnimationType
    {
        Pulse,      // 깜빡임
        Rotate,     // 회전
        Scale       // 크기 변화
    }

    private GameObject borderObject;
    private Image borderImage;
    private RectTransform borderRectTransform;
    private AlarmLevel currentAlarmLevel = AlarmLevel.Normal;
    private Coroutine animationCoroutine;
    private SensorModel sensorData;

    void Start()
    {
        CreateBorder();
        SetAlarmLevel(AlarmLevel.Normal);
        StartCoroutine(InitializeAfterDataLoad());
        StartCoroutine(CheckAlarmStatus());
    }

    IEnumerator InitializeAfterDataLoad()
    {
        while (UiManager.Instance == null ||
               UiManager.Instance.modelProvider == null ||
               UiManager.Instance.modelProvider.GetSensors().Count == 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        var sensors = UiManager.Instance.modelProvider.GetSensors();
        sensorData = sensors.FirstOrDefault(s => s.sensor_id == sensorId);

        if (sensorData == null)
        {
            Debug.LogError($"센서 {sensorId} 데이터를 찾을 수 없음!");
        }
        else
        {
            Debug.Log($"센서 {sensorId} 초기화 성공: {sensorData.sensor_name}");
        }
    }

    IEnumerator CheckAlarmStatus()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (sensorData != null && UiManager.Instance?.modelProvider != null)
            {
                try
                {
                    List<AlarmLogModel> alarms = UiManager.Instance.modelProvider.GetAlarmLogList()
                        .Where(log => log.sensor_id == sensorId && string.IsNullOrEmpty(log.solved_time))
                        .ToList();

                    Debug.Log($"센서 {sensorId} 활성 알람 개수: {alarms.Count}");

                    // 직접 문자열 비교로 변경
                    bool hasWarning = alarms.Any(alarm => alarm.alarm_level == "Warning");
                    bool hasSerious = alarms.Any(alarm => alarm.alarm_level == "Serious");

                    if (hasWarning)
                    {
                        SetAlarmLevel(AlarmLevel.Alert);
                        Debug.Log($"센서 {sensorId}: 경보(Warning) 테두리 빨간색");
                    }
                    else if (hasSerious)
                    {
                        SetAlarmLevel(AlarmLevel.Warning);
                        Debug.Log($"센서 {sensorId}: 경계(Serious) 테두리 노란색");
                    }
                    else
                    {
                        SetAlarmLevel(AlarmLevel.Normal);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"알람 체크 중 에러: {e.Message}");
                }
            }

            yield return new WaitForSeconds(3f);
        }
    }

    void CreateBorder()
    {
        borderObject = new GameObject("AlarmBorder");
        borderObject.transform.SetParent(transform, false);

        borderRectTransform = borderObject.AddComponent<RectTransform>();
        borderRectTransform.anchorMin = Vector2.zero;
        borderRectTransform.anchorMax = Vector2.one;

        if (!useInnerBorder)
        {
            borderRectTransform.sizeDelta = new Vector2(borderWidth * 2, borderWidth * 2);
            borderRectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            borderRectTransform.sizeDelta = Vector2.zero;
            borderRectTransform.anchoredPosition = Vector2.zero;
        }

        borderImage = borderObject.AddComponent<Image>();
        CreateBorderSprite();
        borderObject.transform.SetAsFirstSibling();
        borderObject.SetActive(false);
    }

    void CreateBorderSprite()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        int borderPixels = Mathf.RoundToInt(borderWidth * 2);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isHorizontalBorder = (x < borderPixels || x >= size - borderPixels);
                bool isVerticalBorder = (y < borderPixels || y >= size - borderPixels);

                if (isHorizontalBorder || isVerticalBorder)
                {
                    float distanceFromEdgeX = Mathf.Min(x, size - x - 1);
                    float distanceFromEdgeY = Mathf.Min(y, size - y - 1);
                    float minDistance = Mathf.Min(distanceFromEdgeX, distanceFromEdgeY);
                    float alpha = Mathf.Clamp01(minDistance / 2f);
                    pixels[x + y * size] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[x + y * size] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite borderSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.Tight,
            new Vector4(borderPixels, borderPixels, borderPixels, borderPixels)
        );

        borderImage.sprite = borderSprite;
        borderImage.type = Image.Type.Sliced;
        borderImage.pixelsPerUnitMultiplier = 1f;
    }

    public void SetAlarmLevel(AlarmLevel level)
    {
        if (currentAlarmLevel == level) return;

        currentAlarmLevel = level;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;

            if (borderRectTransform != null)
            {
                borderRectTransform.localRotation = Quaternion.identity;
                borderRectTransform.localScale = Vector3.one;
            }
        }

        switch (level)
        {
            case AlarmLevel.Normal:
                borderObject.SetActive(false);
                break;

            case AlarmLevel.Warning:
                borderObject.SetActive(true);
                borderImage.color = warningColor;
                if (useAnimation)
                {
                    animationCoroutine = animationType switch
                    {
                        AnimationType.Pulse => StartCoroutine(PulseAnimation(warningColor)),
                        AnimationType.Rotate => StartCoroutine(RotateAnimation(warningColor)),
                        AnimationType.Scale => StartCoroutine(ScaleAnimation(warningColor)),
                        _ => StartCoroutine(PulseAnimation(warningColor))
                    };
                }
                break;

            case AlarmLevel.Alert:
                borderObject.SetActive(true);
                borderImage.color = alertColor;
                if (useAnimation)
                {
                    animationCoroutine = animationType switch
                    {
                        AnimationType.Pulse => StartCoroutine(PulseAnimation(alertColor)),
                        AnimationType.Rotate => StartCoroutine(RotateAnimation(alertColor)),
                        AnimationType.Scale => StartCoroutine(ScaleAnimation(alertColor)),
                        _ => StartCoroutine(PulseAnimation(alertColor))
                    };
                }
                break;
        }
    }

    IEnumerator PulseAnimation(Color baseColor)
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * animationSpeed) + 1f) / 2f;
            Color newColor = baseColor;
            newColor.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            borderImage.color = newColor;
            yield return null;
        }
    }

    IEnumerator RotateAnimation(Color baseColor)
    {
        borderImage.color = baseColor;
        while (true)
        {
            borderRectTransform.Rotate(0, 0, animationSpeed * 30 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator ScaleAnimation(Color baseColor)
    {
        borderImage.color = baseColor;
        Vector3 originalScale = Vector3.one;

        while (true)
        {
            float t = (Mathf.Sin(Time.time * animationSpeed) + 1f) / 2f;
            float scale = Mathf.Lerp(0.95f, 1.05f, t);
            borderRectTransform.localScale = originalScale * scale;
            yield return null;
        }
    }

    [ContextMenu("Test Warning")]
    void TestWarning() => SetAlarmLevel(AlarmLevel.Warning);

    [ContextMenu("Test Alert")]
    void TestAlert() => SetAlarmLevel(AlarmLevel.Alert);

    [ContextMenu("Reset to Normal")]
    void TestNormal() => SetAlarmLevel(AlarmLevel.Normal);
}