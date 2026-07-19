using UnityEngine;
public class RaiseFists {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand").transform; var lh=GameObject.Find("LeftHand").transform;
    wh.localPosition=new Vector3(0.25f,-0.15f,0.45f);
    lh.localPosition=new Vector3(-0.25f,-0.15f,0.45f);
    Transform rFist=null,sword=null; foreach(Transform c in wh){ var n=c.name.ToLower(); if(n.Contains("right"))rFist=c; if(n.Contains("sword"))sword=c; }
    var torch=lh.Find("Torch"); var lFist=lh.GetChild(0);
    string tinfo="no torch";
    if(sword!=null){ Bounds sb=RB(sword),fb=RB(rFist); sword.position += (fb.center - new Vector3(sb.center.x,sb.min.y,sb.center.z)) + wh.up*0.03f; }
    if(torch!=null){ Bounds tb=RB(torch),fb=RB(lFist); torch.position += (fb.center - new Vector3(tb.center.x,tb.min.y,tb.center.z)) + lh.up*0.02f; tinfo="torch world="+RB(torch).center.ToString("0.00"); }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "fists raised; "+tinfo;
  }
}
