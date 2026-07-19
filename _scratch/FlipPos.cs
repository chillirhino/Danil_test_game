using UnityEngine;
public class FlipPos {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static void Curl(Transform root,float ang){ string[] fk={"Index","Middle","Ring","Pinky","Thumb"}; foreach(var t in root.GetComponentsInChildren<Transform>()){ foreach(var k in fk){ if(t.name.Contains(k)){ t.localRotation*=Quaternion.Euler(ang,0,0); break; } } } }
  public static string Main(){
    var cam=GameObject.Find("Main Camera").transform;
    var wh=GameObject.Find("WeaponHand").transform; var lh=GameObject.Find("LeftHand").transform;
    Transform rHand=null,sword=null; foreach(Transform c in wh){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    var lHand=lh.GetChild(0);
    // flip fingers to the other side (from +55 to -55)
    Curl(rHand,-110f); Curl(lHand,-110f);
    // reposition by measure so they stay in frame
    wh.position += cam.TransformPoint(new Vector3(0.18f,-0.22f,0.5f)) - RB(rHand).center;
    lh.position += cam.TransformPoint(new Vector3(-0.18f,-0.22f,0.5f)) - RB(lHand).center;
    if(sword!=null){ Bounds sb=RB(sword),fb=RB(rHand); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.03f; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "fingers flipped + repositioned";
  }
}
