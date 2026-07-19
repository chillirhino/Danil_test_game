using UnityEngine;
using UnityEditor;
public class AddTorch {
  static Material Mat(string name, Color c, bool emis){
    string p="Assets/Art/Materials/"+name+".mat";
    var m=AssetDatabase.LoadAssetAtPath<Material>(p);
    if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
    m.SetColor("_BaseColor",c); if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0f);
    if(emis){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(2f,0.8f,0.15f));}
    EditorUtility.SetDirty(m); return m;
  }
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static GameObject Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Material m){
    var g=GameObject.CreatePrimitive(t); g.transform.SetParent(parent,false); g.transform.localPosition=pos; g.transform.localScale=scale;
    Object.DestroyImmediate(g.GetComponent<Collider>()); g.GetComponent<Renderer>().sharedMaterial=m; return g;
  }
  public static string Main(){
    var lh=GameObject.Find("LeftHand").transform;
    var old=lh.Find("Torch"); if(old!=null) Object.DestroyImmediate(old.gameObject);
    var wood=Mat("TorchWood",new Color(0.28f,0.18f,0.10f),false);
    var flame=Mat("Flame",new Color(1f,0.55f,0.12f),true);
    var torch=new GameObject("Torch"); torch.transform.SetParent(lh,false);
    // handle (vertical) + flame on top + light
    Prim(PrimitiveType.Cylinder, torch.transform, new Vector3(0,0,0), new Vector3(0.03f,0.16f,0.03f), wood);
    Prim(PrimitiveType.Sphere, torch.transform, new Vector3(0,0.22f,0), new Vector3(0.09f,0.14f,0.09f), flame);
    var lg=new GameObject("TorchLight"); lg.transform.SetParent(torch.transform,false); lg.transform.localPosition=new Vector3(0,0.22f,0);
    var li=lg.AddComponent<Light>(); li.type=LightType.Point; li.color=new Color(1f,0.6f,0.25f); li.range=7f; li.intensity=3f;
    // place torch handle into the LEFT fist (data-driven)
    var lHand=lh.GetChild(0); // hand mesh
    Bounds tb=RB(torch.transform), fb=RB(lHand);
    Vector3 handleWorld=new Vector3(tb.center.x, tb.min.y, tb.center.z);
    torch.transform.position += (fb.center - handleWorld) + lh.up*0.02f;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "torch added to left fist";
  }
}
