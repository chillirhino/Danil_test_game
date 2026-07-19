using UnityEngine;
public class FlipCurl {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static void Curl(Transform root,float ang){ string[] fk={"Index","Middle","Ring","Pinky","Thumb"}; foreach(var t in root.GetComponentsInChildren<Transform>()){ foreach(var k in fk){ if(t.name.Contains(k)){ t.localRotation*=Quaternion.Euler(ang,0,0); break; } } } }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand").transform; var lh=GameObject.Find("LeftHand").transform;
    Transform rFist=null,sword=null; foreach(Transform c in wh){ var n=c.name.ToLower(); if(n.Contains("right"))rFist=c; if(n.Contains("sword"))sword=c; }
    var lFist=lh.GetChild(0);
    Curl(rFist,-90f); Curl(lFist,-90f); // flip curl to the other side
    if(sword!=null){ Bounds sb=RB(sword),fb=RB(rFist); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.03f; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "curl flipped";
  }
}
