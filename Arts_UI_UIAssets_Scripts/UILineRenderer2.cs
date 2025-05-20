using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
//using Palmmedia.ReportGenerator.Core;
using Onthesys;
using DG.Tweening;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer2 : MaskableGraphic
{
    public List<Transform> controlPointsObjects = new List<Transform>(); // ���Ƶ�����б�
    protected List<Vector2> controlPoints = new List<Vector2>(); // ���Ƶ�λ���б�
    public List<TMP_Text> hours;
    public float thickness = 2f; // ������ϸ

    #region [Temp]

    (float max, float min) GetDotRangeOfX() 
    {
        Transform hLineContainer = transform.parent.Find("Chart_Grid").Find("Lines_Horizon");

        if (hLineContainer == null) throw new Exception($"UILineRenderer2({gameObject.name}) - GetDotRangeOfX : hLineContainer를 찾지 못했습니다.");
        if (hLineContainer.childCount < 2) throw new Exception($"UILineRenderer2({gameObject.name}) - GetDotRangeOfX :  x좌표 범위를 계산하기 위해 충분한 hLineContainer의 자식을 찾지 못했습니다.");

        (float min, float max) dotRangeOfX = (
            hLineContainer.Find("Image").position.x,
            hLineContainer.Find("Image (6)").position.x);

        return dotRangeOfX;
    }

    public bool SetDotAmount(int amount) 
    {

        (float min, float max) dotRangeOfX = GetDotRangeOfX();

        try {
            //if (amount < 2) throw new Exception("그래프 점 개수가 너무 적습니다. 2 이상을 선언해주세요.");
            if (transform.childCount <= 0) throw new Exception($"UILineRenderer2({gameObject.name}) - SetDotAmount : 적합한 점 프리팹 대상을 찾지 못했습니다. 관리자에게 해당 메세지를 보여주세요.");

            GameObject dotPrefab = transform.GetChild(0).gameObject;

            while (transform.childCount > 0)
            {
                var oldDot = transform.GetChild(0);
                controlPointsObjects.Remove(oldDot);
                oldDot.SetParent(null);
                if (oldDot != dotPrefab.transform) Destroy(oldDot.gameObject);
            }

            for (int i = 0; i < amount; i++)
            { 
                var newDot = Instantiate<GameObject>(dotPrefab);
                newDot.transform.SetParent(transform);
                newDot.transform.localScale = Vector3.one * 0.5f;
                controlPointsObjects.Add(newDot.transform);
            }
            Destroy(dotPrefab); 

            

            for (int i = 0; i < controlPointsObjects.Count; i++)
            {
                Transform dot = controlPointsObjects[i];
                float ratio = (float)i / (controlPointsObjects.Count - 1);
                //Debug.Log($"ratio[{i}] : {ratio} {Mathf.Lerp(dotRangeOfX.min, dotRangeOfX.max, ratio)}");
                dot.position = new Vector3()
                {
                    x = Mathf.Lerp(dotRangeOfX.min, dotRangeOfX.max, ratio),
                    y = dot.position.y,
                    z = dot.position.z,
                };
            }
        }
        catch (Exception ex){
            Debug.LogException(ex);
            return false;
        }

        return true;
    }

    #endregion



    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); // ���֮ǰ�Ķ�������
        //CreateControlPoints(); // ���¿��Ƶ�λ��
        if (controlPoints.Count < 2) return; // ������Ҫ�������������߶�

        DrawLines(vh, controlPoints);
    }

    void Update()
    {
        SetVerticesDirty(); // ʵʱ�����߶�
    }

    void SetHours()
    {
        int hour = DateTime.Now.Hour;
        for (int i = 0; i < this.hours.Count; i++)
        {
            this.hours[i].text = hour.ToString() + ":00";
            hour -= 3;
            if (hour <= 0)
            {
                hour = 24 - (3 - hour - 1);
            }
        }
    }

    void SetMins(DateTime dt)
    {
        DateTime startDt = dt.AddHours(-4);
        var turm = (dt - startDt).TotalMinutes / this.hours.Count;

        for(int i = 0; i < this.hours.Count ; i++)
        {
            var t = dt.AddMinutes(-(turm * i));
            this.hours[i].text = t.ToString("HH:mm");
        }
    }

    private void CreateControlPoints()
    {
        controlPoints.Clear();
        foreach (Transform controlPointObject in controlPointsObjects)
        {
            if (controlPointObject != null)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, controlPointObject.position);
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvas.worldCamera, out localPoint);
                controlPoints.Add(localPoint);
            }
        }
    }

    public void UpdateControlPoints(List<float> points)
    {

        //ui 구성이 완료되지 않았다면 지연
        (float min, float max) dotRangeOfX = GetDotRangeOfX();
        if (dotRangeOfX.max - dotRangeOfX.min < 0.1f)
        {
            DOVirtual.DelayedCall(0.1f, ()=> UpdateControlPoints(points));
            return;
        }

        if (controlPointsObjects.Count != points.Count) SetDotAmount(points.Count);

        controlPoints.Clear();
        for (int i = 0; i < this.controlPointsObjects.Count; i++)
        {
            if (i >= points.Count)
                break;
            if (this.controlPointsObjects[i] != null)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, this.controlPointsObjects[i].position);
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvas.worldCamera, out localPoint);
                localPoint.y = -this.GetComponent<RectTransform>().sizeDelta.y - (-this.GetComponent<RectTransform>().sizeDelta.y * points[i]) + (this.GetComponent<RectTransform>().sizeDelta.y / 2);

                Vector2 vPos = this.controlPointsObjects[i].GetComponent<RectTransform>().anchoredPosition;
                vPos.y = localPoint.y + (this.GetComponent<RectTransform>().sizeDelta.y / 2);
                this.controlPointsObjects[i].GetComponent<RectTransform>().anchoredPosition = vPos;

                controlPoints.Add(localPoint);
            }
        }
        //this.SetHours();
    }

    public void UpdateControlPoints(DateTime dt, List<float> points)
    {
        controlPoints.Clear();
        for (int i = 0; i < this.controlPointsObjects.Count; i++) 
        {
                if (i >= points.Count)
                    break;
            if (this.controlPointsObjects[i] != null)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, this.controlPointsObjects[i].position);
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvas.worldCamera, out localPoint);
                localPoint.y = -this.GetComponent<RectTransform>().sizeDelta.y - (-this.GetComponent<RectTransform>().sizeDelta.y * points[i]) + (this.GetComponent<RectTransform>().sizeDelta.y / 2);

                Vector2 vPos = this.controlPointsObjects[i].GetComponent<RectTransform>().anchoredPosition;
                vPos.y = localPoint.y + (this.GetComponent<RectTransform>().sizeDelta.y / 2);
                this.controlPointsObjects[i].GetComponent<RectTransform>().anchoredPosition = vPos;

                controlPoints.Add(localPoint);
            }
        }
        this.SetMins(dt);
    }

    private void DrawLines(VertexHelper vh, List<Vector2> points)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            AddVerticesForLineSegment(vh, points[i], points[i + 1], thickness);
        }
    }

    private void AddVerticesForLineSegment(VertexHelper vh, Vector2 start, Vector2 end, float thickness)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * thickness / 2;
        vh.AddVert(start + normal, color, new Vector2(0, 0));
        vh.AddVert(start - normal, color, new Vector2(0, 1));
        vh.AddVert(end - normal, color, new Vector2(1, 1));
        vh.AddVert(end + normal, color, new Vector2(1, 0));

        int baseIndex = vh.currentVertCount;
        vh.AddTriangle(baseIndex - 4, baseIndex - 3, baseIndex - 2);
        vh.AddTriangle(baseIndex - 2, baseIndex - 1, baseIndex - 4);
    }
}
