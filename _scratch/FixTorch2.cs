using UnityEngine;
public class FixTorch2 {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand").transform;
    var lh=GameObject.Find("LeftHand").transform;
    wh.localPosition=new Vector3(0.25f,-0.19f,0.46f);
    lh.localPosition=new Vector3(-0.25f,-0.19f,0.46f);
    // torch upright, slight left
    var torch=lh.Find("Torch"); var lFist=lh.GetChild(0);
    Transform rFist=null,sword=null; foreach(Transform c in wh){ var n=c.name.ToLower(); if(n.Contains("right"))rFist=c; if(n.Contains("sword"))sword=c; }
    if(torch!=null){ torch.localRotation=Quaternion.Euler(-5f,0f,-6f); Bounds tb=RB(torch),fb=RB(lFist); torch.position += (fb.center - new Vector3(tb.center.x,tb.min.y,tb.center.z)) + lh.up*0.02f; }
    if(sword!=null){ Bounds sb=RB(sword),fb=RB(rFist); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.03f; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "torch upright + hands raised";
  }
}
