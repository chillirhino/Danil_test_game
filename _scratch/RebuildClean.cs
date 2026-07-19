using UnityEngine;
using UnityEditor;
public class RebuildClean {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static void Curl(Transform root,float ang){ string[] fk={"Index","Middle","Ring","Pinky","Thumb"}; foreach(var t in root.GetComponentsInChildren<Transform>()){ foreach(var k in fk){ if(t.name.Contains(k)){ t.localRotation*=Quaternion.Euler(ang,0,0); break; } } } }
  public static string Main(){
    var hand=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleHands/Prefabs/WhiteHand.prefab");
    var wh=GameObject.Find("WeaponHand").transform; var lh=GameObject.Find("LeftHand").transform;
    Transform sword=null; foreach(Transform c in wh){ if(c.name.ToLower().Contains("sword"))sword=c; }
    // wipe children except sword
    for(int i=wh.childCount-1;i>=0;i--){ var c=wh.GetChild(i); if(!c.name.ToLower().Contains("sword")) Object.DestroyImmediate(c.gameObject); }
    for(int i=lh.childCount-1;i>=0;i--) Object.DestroyImmediate(lh.GetChild(i).gameObject);
    // corners, face forward
    wh.localPosition=new Vector3(0.25f,-0.15f,0.45f); wh.localRotation=Quaternion.Euler(0f,0f,10f);
    lh.localPosition=new Vector3(-0.25f,-0.15f,0.45f); lh.localRotation=Quaternion.Euler(0f,0f,-10f);
    var r=(GameObject)PrefabUtility.InstantiatePrefab(hand,wh); r.name="RightHand"; r.transform.localScale=Vector3.one*0.25f; r.transform.localRotation=Quaternion.Euler(-85f,0f,0f);
    var l=(GameObject)PrefabUtility.InstantiatePrefab(hand,lh); l.name="LeftHandMesh"; l.transform.localScale=new Vector3(-0.25f,0.25f,0.25f); l.transform.localRotation=Quaternion.Euler(-85f,0f,0f);
    // curl fingers the OTHER way (negative)
    Curl(r.transform,-40f); Curl(l.transform,-40f);
    // sword into right fist
    if(sword!=null){ sword.SetParent(wh,true); sword.localScale=Vector3.one*0.62f; sword.localRotation=Quaternion.Euler(12f,0f,16f); Bounds sb=RB(sword),fb=RB(r.transform); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.02f; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    var rb=RB(r.transform);
    return "rebuilt; rightHand camY visible, curl=-40; handWorld="+rb.center.ToString("0.00");
  }
}
