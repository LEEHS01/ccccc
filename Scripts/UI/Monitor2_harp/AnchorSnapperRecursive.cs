#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AnchorSnapperRecursive : MonoBehaviour
{
    [MenuItem("Tools/UI/Set Anchors Exactly (Recursive, Preserve Layout)")]
    private static void SnapAnchorsRecursive()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || selected.GetComponent<RectTransform>() == null)
        {
            Debug.LogWarning("RectTransform이 있는 GameObject를 선택하세요.");
            return;
        }

        RectTransform root = selected.GetComponent<RectTransform>();
        ProcessRecursively(root);

        Debug.Log("✔ 앵커가 모든 자식에 재귀적으로 적용되었고, 시각적 레이아웃은 유지되었습니다.");
    }

    public static void ProcessRecursively(RectTransform parent)
    {

        foreach (Transform childTransform in parent)
        {
            RectTransform child = childTransform as RectTransform;
            if (child == null) continue;
            if (parent.GetComponent<LayoutGroup>() != null) continue;


            Undo.RecordObject(child, "Snap Anchors");

            Vector2 parentSize = parent.rect.size;

            // 자식 위치/크기를 기준으로 anchorMin/max 계산
            Vector2 newAnchorMin = new Vector2(
                (child.localPosition.x - child.rect.width * child.pivot.x) / parentSize.x + parent.pivot.x,
                (child.localPosition.y - child.rect.height * child.pivot.y) / parentSize.y + parent.pivot.y
            );

            Vector2 newAnchorMax = new Vector2(
                (child.localPosition.x + child.rect.width * (1 - child.pivot.x)) / parentSize.x + parent.pivot.x,
                (child.localPosition.y + child.rect.height * (1 - child.pivot.y)) / parentSize.y + parent.pivot.y
            );

            // 앵커 설정
            child.anchorMin = newAnchorMin;
            child.anchorMax = newAnchorMax;

            // 마진 초기화 (Top/Bottom/Left/Right = 0)
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;

            // 하위 자식도 재귀 처리
            ProcessRecursively(child);
        }
    }
}

public class AnchorSnapperCenter : MonoBehaviour
{
    [MenuItem("Tools/UI/Set Anchors to Center (Recursive, Preserve Size)")]
    private static void SnapAnchorsToCenterRecursive()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || selected.GetComponent<RectTransform>() == null)
        {
            Debug.LogWarning("RectTransform이 있는 GameObject를 선택하세요.");
            return;
        }

        RectTransform root = selected.GetComponent<RectTransform>();
        ProcessRecursively(root);

        Debug.Log("✔ 앵커가 자식들의 정중앙에 설정되고, 크기도 정확히 유지되었습니다.");
    }

    private static void ProcessRecursively(RectTransform parent)
    {
        foreach (Transform childTransform in parent)
        {
            RectTransform child = childTransform as RectTransform;
            if (child == null) continue;
            if (parent.GetComponent<LayoutGroup>() != null) continue;

            Undo.RecordObject(child, "Snap Anchors to Center");

            Vector2 parentSize = parent.rect.size;
            if (Mathf.Approximately(parentSize.x, 0) || Mathf.Approximately(parentSize.y, 0))
                continue;

            // 현재 크기 계산
            Vector2 size = child.rect.size;

            // 중심점(anchor 중앙) 계산
            Vector2 centerAnchor = new Vector2(
                (child.localPosition.x) / parentSize.x + parent.pivot.x,
                (child.localPosition.y) / parentSize.y + parent.pivot.y
            );

            // 앵커를 점으로 설정
            child.anchorMin = centerAnchor;
            child.anchorMax = centerAnchor;

            // offsetMin/offsetMax를 width/height 기반으로 설정
            child.offsetMin = new Vector2(-size.x / 2f, -size.y / 2f);
            child.offsetMax = new Vector2(size.x / 2f, size.y / 2f);

            // 하위 자식들도 재귀 적용
            ProcessRecursively(child);
        }
    }
}

#endif