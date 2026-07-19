using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;

public class FixEventSystem
{
    public static string Main()
    {
        var es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null) return "no EventSystem";

        var old = es.GetComponent<StandaloneInputModule>();
        if (old != null) Object.DestroyImmediate(old);

        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(es.gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(es.gameObject.scene);
        return "ok replaced input module";
    }
}
