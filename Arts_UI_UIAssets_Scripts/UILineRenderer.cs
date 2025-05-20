using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using DG.Tweening;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : MaskableGraphic
{
    /// <summary>
    /// controlPointsObjects ====> 초기화 시 바로 24개가 생기는건지?????? 문의가 필요해보임
    /// </summary>
    public List<Transform> controlPointsObjects = new List<Transform>(); // ���Ƶ�����б�
    private List<Vector2> controlPoints = new List<Vector2>(); // ���Ƶ�λ���б�
    public float thickness = 2f; // ������ϸ

    #region [Temp]

    (float max, float min) GetDotRangeOfX()
    {
        Rect rect = GetComponent<RectTransform>().rect;
        (float min, float max) dotRangeOfX = (
            transform.position.x,
            transform.position.x + rect.width
            );

        return dotRangeOfX;
    }

    public bool SetDotAmount(int amount)
    {

        (float min, float max) dotRangeOfX = GetDotRangeOfX();

        try
        {
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
        catch (Exception ex)
        {
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
            DOVirtual.DelayedCall(0.1f, () => UpdateControlPoints(points));
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
                //localPoint.y = -18f - (-18f * points[i]);
                localPoint.y = -this.GetComponent<RectTransform>().sizeDelta.y 
                    - (-this.GetComponent<RectTransform>().sizeDelta.y * points[i]) 
                    + (this.GetComponent<RectTransform>().sizeDelta.y / 2);

                Vector2 vPos = this.controlPointsObjects[i].GetComponent<RectTransform>().anchoredPosition;
                vPos.y = localPoint.y + (this.GetComponent<RectTransform>().sizeDelta.y / 2);
                this.controlPointsObjects[i].GetComponent<RectTransform>().anchoredPosition = vPos;

                controlPoints.Add(localPoint);
            }
        }
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
