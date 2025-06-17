using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class PrefabOverrideReverter : EditorWindow
{
    [MenuItem("Tools/Prefab/Revert All Overrides (Recursive - Legacy)")]
    private static void RevertOverridesLegacy()
    {
        var selection = Selection.gameObjects;
        if (selection.Length == 0)
        {
            Debug.LogWarning("선택된 오브젝트가 없습니다.");
            return;
        }

        int count = 0;

        foreach (var root in selection)
        {
            count += RevertRecursive(root);
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"Reverted overrides on {count} GameObject(s).");
    }

    private static int RevertRecursive(GameObject obj)
    {
        int revertCount = 0;

        if (PrefabUtility.IsPartOfPrefabInstance(obj))
        {
            var overrides = PrefabUtility.GetObjectOverrides(obj);
            foreach (var overr in overrides)
            {
                PrefabUtility.RevertObjectOverride(overr.instanceObject, InteractionMode.UserAction);
                revertCount++;
            }

            var added = PrefabUtility.GetAddedGameObjects(obj);
            foreach (var add in added)
            {
                PrefabUtility.RevertAddedGameObject(add.instanceGameObject, InteractionMode.UserAction);
                revertCount++;
            }

            var removed = PrefabUtility.GetRemovedComponents(obj);
            foreach (var removedComponent in removed)
            {
                PrefabUtility.RevertRemovedComponent(obj, removedComponent.assetComponent, InteractionMode.UserAction);
                revertCount++;
            }
        }

        foreach (Transform child in obj.transform)
        {
            revertCount += RevertRecursive(child.gameObject);
        }

        return revertCount;
    }
}
