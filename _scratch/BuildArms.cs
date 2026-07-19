using UnityEngine;
using UnityEditor;
public class BuildArms {
  static Material Mat(string name, Color c){
    string p="Assets/Art/Materials/"+name+".mat";
    var m=AssetDatabase.LoadAssetAtPath<Material>(p);
    if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
    m.SetColor("_BaseColor",c); if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0f);
    EditorUtility.SetDirty(m); return m;
  }
  static void Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Vector3 rot, Material m){
    var g=GameObject.CreatePrimitive(t); g.transform.SetParent(parent,false);
    g.transform.localPosition=pos; g.transform.localScale=scale; g.transform.localEulerAngles=rot;
    Object.DestroyImmediate(g.GetComponent<Collider>());
    g.GetComponent<Renderer>().sharedMaterial=m;
  }
  static void Arm(Transform parent){
    var skin=Mat("Skin",new Color(0.72f,0.52f,0.38f));
    var leather=Mat("Leather",new Color(0.28f,0.18f,0.10f));
    // forearm (bracer) receding down-back from the grip
    Prim(PrimitiveType.Cylinder, parent, new Vector3(0f,-0.22f,-0.02f), new Vector3(0.06f,0.22f,0.06f), new Vector3(18f,0f,0f), leather);
    // fist at the grip
    Prim(PrimitiveType.Sphere, parent, new Vector3(0f,-0.01f,0f), new Vector3(0.11f,0.09f,0.12f), Vector3.zero, skin);
  }
  public static string Main(){
    var hand=GameObject.Find("WeaponHand");
    if(hand==null) return "no WeaponHand";
    var baked=hand.transform.Find("RightArm"); if(baked!=null) Object.DestroyImmediate(baked.gameObject);
    var arm=new GameObject("RightArm"); arm.transform.SetParent(hand.transform,false);
    Arm(arm.transform);
    AssetDatabase.SaveAssets();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "primitive right arm built";
  }
}
