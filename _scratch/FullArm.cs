using UnityEngine;
using UnityEditor;
public class FullArm {
  static Material _steel, _sleeve;
  static Material Mat(string name, Color c, float sm){
    string p="Assets/Art/Materials/"+name+".mat";
    var m=AssetDatabase.LoadAssetAtPath<Material>(p);
    if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
    m.SetColor("_BaseColor",c); if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",sm);
    EditorUtility.SetDirty(m); return m;
  }
  static void AddForearm(Transform hand, float side){
    // remove old forearm if present
    var old=hand.Find("Forearm"); if(old!=null) Object.DestroyImmediate(old.gameObject);
    var f=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    f.name="Forearm"; f.transform.SetParent(hand,false);
    Object.DestroyImmediate(f.GetComponent<Collider>());
    // from the wrist down-back toward the elbow (off bottom of screen)
    f.transform.localPosition=new Vector3(0.01f*side,-0.34f,-0.03f);
    f.transform.localScale=new Vector3(0.085f,0.34f,0.085f);
    f.transform.localEulerAngles=new Vector3(16f,0f,-6f*side);
    f.GetComponent<Renderer>().sharedMaterial=_sleeve;
    // an elbow/cuff ring where sleeve meets gauntlet
    var cuff=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    cuff.name="Cuff"; cuff.transform.SetParent(hand,false);
    Object.DestroyImmediate(cuff.GetComponent<Collider>());
    cuff.transform.localPosition=new Vector3(0f,-0.09f,-0.01f);
    cuff.transform.localScale=new Vector3(0.11f,0.05f,0.11f);
    cuff.transform.localEulerAngles=new Vector3(12f,0f,0f);
    cuff.GetComponent<Renderer>().sharedMaterial=_steel;
  }
  public static string Main(){
    _steel=Mat("ArmSteel",new Color(0.46f,0.48f,0.54f),0.35f);
    _sleeve=Mat("ArmSleeve",new Color(0.22f,0.17f,0.13f),0.05f); // dark leather/cloth sleeve
    var vm=GameObject.Find("ViewModel");
    var wh=vm.transform.Find("WeaponHand");
    var lh=vm.transform.Find("LeftHand");
    AddForearm(wh,1f);
    AddForearm(lh,-1f);
    AssetDatabase.SaveAssets();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "full forearms added";
  }
}
