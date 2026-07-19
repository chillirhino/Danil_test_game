using UnityEngine;
public class FaceFwd {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static void Curl(Transform root,float ang){ string[] fk={"Index","Middle","Ring","Pinky","Thumb"}; foreach(var t in root.GetComponentsInChildren<Transform>()){ foreach(var k in fk){ if(t.name.Contains(k)){ t.localRotation*=Quaternion.Euler(ang,0,0); break; } } } }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand"); var lh=GameObject.Find("LeftHand").transform;
    Transform rHand=null,sword=null;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    var lHand=lh.GetChild(0);
    // rotate hands so fingers point forward (fists face forward)
    rHand.localRotation=Quaternion.Euler(-85f,0f,0f);
    lHand.localRotation=Quaternion.Euler(-85f,0f,0f);
    Curl(rHand,45f); Curl(lHand,45f);
    // re-place sword handle into the fist (data-driven)
    sword.localScale=Vector3.one*0.65f; sword.localRotation=Quaternion.Euler(14f,-6f,-24f);
    Bounds sb=RB(sword), fb=RB(rHand);
    sword.position += (fb.center - new Vector3(sb.center.x, sb.min.y, sb.center.z)) + wh.transform.up*0.03f;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "hands face forward + curled, sword re-placed";
  }
}
