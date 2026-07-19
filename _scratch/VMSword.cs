using UnityEngine;
using UnityEditor;
public class VMSword {
  static string P(string kw){ foreach(var g in AssetDatabase.FindAssets("t:Prefab")){var p=AssetDatabase.GUIDToAssetPath(g);if(p.Contains("Polytope")&&p.ToLower().Contains(kw))return p;}return null; }
  public static string Main(){
    var cam=GameObject.Find("Main Camera");
    var oldVM=GameObject.Find("ViewModel"); if(oldVM!=null) Object.DestroyImmediate(oldVM);
    var vm=new GameObject("ViewModel"); vm.transform.SetParent(cam.transform,false);
    var sword=AssetDatabase.LoadAssetAtPath<GameObject>(P("sword_01_a"));
    if(sword==null) return "no sword";
    var s=(GameObject)PrefabUtility.InstantiatePrefab(sword,vm.transform);
    s.transform.localPosition=Vector3.zero; s.transform.localRotation=Quaternion.identity; s.transform.localScale=Vector3.one;
    // measure
    bool h=false;Bounds b=new Bounds();foreach(var r in s.GetComponentsInChildren<Renderer>()){if(!h){b=r.bounds;h=true;}else b.Encapsulate(r.bounds);}
    string info="swordBoundsSize="+b.size+" center(local)="+s.transform.InverseTransformPoint(b.center);
    // now pose it: scale to ~0.5 tall, bottom-right, blade up-forward
    float target=0.55f; float sc=target/Mathf.Max(0.01f,b.size.y);
    s.transform.localScale=Vector3.one*sc;
    s.transform.localPosition=new Vector3(0.18f,-0.28f,0.5f);
    s.transform.localRotation=Quaternion.Euler(25f,0f,15f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return info+" | scaled to "+sc.ToString("0.000");
  }
}
