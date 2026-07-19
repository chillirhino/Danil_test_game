using UnityEngine;
using UnityEditor;
public class SwapRocks {
  static float H(GameObject g){bool h=false;Bounds b=new Bounds(g.transform.position,Vector3.zero);foreach(var r in g.GetComponentsInChildren<Renderer>()){if(!h){b=r.bounds;h=true;}else b.Encapsulate(r.bounds);}return h?b.size.y:1f;}
  public static string Main(){
    var rocks=new GameObject[5];
    for(int i=0;i<5;i++) rocks[i]=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleNaturePack/Prefabs/Rock_0"+(i+1)+".prefab");
    var env=GameObject.Find("Environment");
    var old=env.transform.Find("Rocks");
    if(old!=null) Object.DestroyImmediate(old.gameObject);
    var holder=new GameObject("Rocks").transform; holder.SetParent(env.transform,false);
    float[] rz={1f,8f,15f,23f,31f,39f,5f,27f};
    float[] rx={2.6f,-2.7f,2.8f,-2.6f,2.7f,-2.8f,-3.2f,3.1f};
    for(int i=0;i<rz.Length;i++){
      var p=rocks[i%5]; if(p==null) continue;
      var r=(GameObject)PrefabUtility.InstantiatePrefab(p,holder);
      r.transform.localPosition=new Vector3(rx[i],0f,rz[i]);
      r.transform.localRotation=Quaternion.Euler(0f,(i*61f)%360f,0f);
      float h=H(r); float target=0.5f+(i%3)*0.3f;
      r.transform.localScale=Vector3.one*(target/Mathf.Max(0.01f,h));
    }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "swapped to "+rz.Length+" low-poly rocks";
  }
}
