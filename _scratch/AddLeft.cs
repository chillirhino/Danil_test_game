using UnityEngine;
using UnityEditor;
public class AddLeft {
  static Material Mat(string name, Color c, bool emis){
    string p="Assets/Art/Materials/"+name+".mat";
    var m=AssetDatabase.LoadAssetAtPath<Material>(p);
    if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
    m.SetColor("_BaseColor",c); if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0f);
    if(emis){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(1.5f,0.6f,0.1f));}
    EditorUtility.SetDirty(m); return m;
  }
  static GameObject Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Vector3 rot, Material m){
    var g=GameObject.CreatePrimitive(t); g.transform.SetParent(parent,false);
    g.transform.localPosition=pos; g.transform.localScale=scale; g.transform.localEulerAngles=rot;
    Object.DestroyImmediate(g.GetComponent<Collider>()); g.GetComponent<Renderer>().sharedMaterial=m; return g;
  }
  public static string Main(){
    var vm=GameObject.Find("ViewModel");
    if(vm==null) return "no ViewModel";
    var old=vm.transform.Find("LeftHand"); if(old!=null) Object.DestroyImmediate(old.gameObject);
    var skin=Mat("Skin",new Color(0.72f,0.52f,0.38f),false);
    var leather=Mat("Leather",new Color(0.28f,0.18f,0.10f),false);
    var wood=Mat("TorchWood",new Color(0.30f,0.20f,0.11f),false);
    var flame=Mat("Flame",new Color(1f,0.55f,0.12f),true);

    var hand=new GameObject("LeftHand"); hand.transform.SetParent(vm.transform,false);
    hand.transform.localPosition=new Vector3(-0.17f,-0.16f,0.5f);
    hand.transform.localRotation=Quaternion.Euler(0f,0f,-12f);
    // arm
    Prim(PrimitiveType.Cylinder, hand.transform, new Vector3(0f,-0.22f,-0.02f), new Vector3(0.06f,0.22f,0.06f), new Vector3(18f,0f,0f), leather);
    Prim(PrimitiveType.Sphere, hand.transform, new Vector3(0f,-0.01f,0f), new Vector3(0.11f,0.09f,0.12f), Vector3.zero, skin);
    // torch: handle up from fist + flame
    Prim(PrimitiveType.Cylinder, hand.transform, new Vector3(0f,0.14f,0.02f), new Vector3(0.03f,0.16f,0.03f), new Vector3(-8f,0f,0f), wood);
    var f=Prim(PrimitiveType.Sphere, hand.transform, new Vector3(0f,0.34f,0.04f), new Vector3(0.10f,0.15f,0.10f), Vector3.zero, flame);
    // small light for the flame
    var lg=new GameObject("TorchLight"); lg.transform.SetParent(hand.transform,false); lg.transform.localPosition=new Vector3(0f,0.34f,0.04f);
    var li=lg.AddComponent<Light>(); li.type=LightType.Point; li.color=new Color(1f,0.6f,0.25f); li.range=6f; li.intensity=2f;

    AssetDatabase.SaveAssets();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "left hand + torch added";
  }
}
