using UnityEngine;
public class PositionHands {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  public static string Main(){
    var cam=GameObject.Find("Main Camera").transform;
    var wh=GameObject.Find("WeaponHand").transform; var lh=GameObject.Find("LeftHand").transform;
    Transform rHand=null,sword=null; foreach(Transform c in wh){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    var lHand=lh.GetChild(0);
    // move each hand so its bounds center sits at a visible target (bottom corners)
    Vector3 rTarget=cam.TransformPoint(new Vector3(0.18f,-0.22f,0.5f));
    Vector3 lTarget=cam.TransformPoint(new Vector3(-0.18f,-0.22f,0.5f));
    wh.position += rTarget - RB(rHand).center;
    lh.position += lTarget - RB(lHand).center;
    // anchor sword handle into right fist
    if(sword!=null){ Bounds sb=RB(sword),fb=RB(rHand); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.03f; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "hands positioned by measure: R="+cam.InverseTransformPoint(RB(rHand).center).ToString("0.00")+" L="+cam.InverseTransformPoint(RB(lHand).center).ToString("0.00");
  }
}
