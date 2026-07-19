using UnityEngine;
using UnityEditor;
public class RestoreWorking {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static void Curl(Transform root,float ang){ string[] fk={"Index","Middle","Ring","Pinky","Thumb"}; foreach(var t in root.GetComponentsInChildren<Transform>()){ foreach(var k in fk){ if(t.name.Contains(k)){ t.localRotation*=Quaternion.Euler(ang,0,0); break; } } } }
  public static string Main(){
    var hand=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleHands/Prefabs/WhiteHand.prefab");
    var wh=GameObject.Find("WeaponHand").transform; var lh=GameObject.Find("LeftHand").transform;
    Transform sword=null; foreach(Transform c in wh){ if(c.name.ToLower().Contains("sword"))sword=c; }
    for(int i=wh.childCount-1;i>=0;i--){ var c=wh.GetChild(i); if(!c.name.ToLower().Contains("sword")) Object.DestroyImmediate(c.gameObject); }
    for(int i=lh.childCount-1;i>=0;i--) Object.DestroyImmediate(lh.GetChild(i).gameObject);
    wh.localPosition=new Vector3(0.2f,-0.26f,0.46f); wh.localRotation=Quaternion.identity;
    lh.localPosition=new Vector3(-0.2f,-0.26f,0.46f); lh.localRotation=Quaternion.identity;
    var r=(GameObject)PrefabUtility.InstantiatePrefab(hand,wh); r.name="RightHand"; r.transform.localScale=Vector3.one*0.25f; r.transform.localRotation=Quaternion.identity;
    var l=(GameObject)PrefabUtility.InstantiatePrefab(hand,lh); l.name="LeftHandMesh"; l.transform.localScale=new Vector3(-0.25f,0.25f,0.25f); l.transform.localRotation=Quaternion.identity;
    Curl(r.transform,55f); Curl(l.transform,55f);
    if(sword!=null){ sword.SetParent(wh,true); sword.localScale=Vector3.one*0.62f; sword.localRotation=Quaternion.Euler(12f,-6f,-22f); Bounds sb=RB(sword),fb=RB(r.transform); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.03f; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "restored working state (fists + sword, visible)";
  }
}
