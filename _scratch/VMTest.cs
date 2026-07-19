using UnityEngine;
using UnityEditor;
public class VMTest {
  static string P(string kw){ foreach(var g in AssetDatabase.FindAssets("t:Prefab")){var p=AssetDatabase.GUIDToAssetPath(g);if(p.Contains("Polytope")&&p.ToLower().Contains(kw))return p;}return null; }
  public static string Main(){
    var cam=GameObject.Find("Main Camera"); if(cam==null) return "no cam";
    // hide old 2D arms
    foreach(var n in new[]{"HUD/LeftArm","HUD/RightArm"}){var g=GameObject.Find(n); if(g!=null) g.SetActive(false);}
    var oldVM=GameObject.Find("ViewModel"); if(oldVM!=null) Object.DestroyImmediate(oldVM);
    var vm=new GameObject("ViewModel"); vm.transform.SetParent(cam.transform,false);
    var sword=AssetDatabase.LoadAssetAtPath<GameObject>(P("sword_01_a"));
    var gaunt=AssetDatabase.LoadAssetAtPath<GameObject>(P("male_armor_01_a_gauntlets"));
    string info="";
    if(sword!=null){var s=(GameObject)PrefabUtility.InstantiatePrefab(sword,vm.transform); s.transform.localPosition=new Vector3(0.28f,-0.32f,0.6f); s.transform.localRotation=Quaternion.Euler(10f,-10f,10f); s.transform.localScale=Vector3.one*0.15f; info+="sword ok ";}
    if(gaunt!=null){var l=(GameObject)PrefabUtility.InstantiatePrefab(gaunt,vm.transform); l.transform.localPosition=new Vector3(-0.28f,-0.4f,0.55f); l.transform.localRotation=Quaternion.Euler(0f,90f,0f); l.transform.localScale=Vector3.one*0.4f; info+="gauntlet ok";}
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return info;
  }
}
