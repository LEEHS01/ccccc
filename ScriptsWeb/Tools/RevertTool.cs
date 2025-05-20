# if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
public class RevertAllPrefabParameters : EditorWindow
{
    [MenuItem("Tools/Revert All Prefab Parameters")]
    public static void ShowWindow()
    {
        GetWindow<RevertAllPrefabParameters>("Revert All Prefab Parameters");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Revert Tool", EditorStyles.boldLabel);

        if (GUILayout.Button("Revert Selected GameObject (Including Children)"))
        {
            RevertSelectedPrefabWithChildren();
        }

        if (GUILayout.Button("Revert All Prefabs in Scene"))
        {
            RevertAllPrefabsInScene();
        }
    }

    private static void RevertSelectedPrefabWithChildren()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("🚫 No GameObject selected!");
            return;
        }

        // 재귀적으로 Prefab Revert
        int revertCount = RevertPrefabRecursive(selectedObject);

        Debug.Log($"✅ Total Reverted Prefabs: {revertCount}");
    }

    private static int RevertPrefabRecursive(GameObject obj)
    {
        int count = 0;

        // Prefab인지 확인
        if (PrefabUtility.IsPartOfPrefabInstance(obj))
        {
            PrefabUtility.RevertPrefabInstance(obj, InteractionMode.UserAction);
            Debug.Log($"🔄 Reverted: {obj.name}");
            count++;
        }

        // 자식 오브젝트에 대해 재귀 호출
        foreach (Transform child in obj.transform)
        {
            count += RevertPrefabRecursive(child.gameObject);
        }

        return count;
    }

    private static void RevertAllPrefabsInScene()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        int revertCount = 0;
        foreach (var obj in allObjects)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                PrefabUtility.RevertPrefabInstance(obj, InteractionMode.UserAction);
                revertCount++;
            }
        }
        Debug.Log($"✅ Total Reverted Prefabs in Scene: {revertCount}");
    }
}
#endif
