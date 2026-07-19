using UnityEngine;
public class PlaceAngled {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    Transform rHand=null,sword=null;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    sword.SetParent(wh.transform,false);
    sword.localScale=Vector3.one*0.65f;
    sword.localRotation=Quaternion.Euler(14f,-6f,-24f);
    // measure and move handle (bounds bottom) to the fist center
    Bounds sb=RB(sword);
    Bounds fb=RB(rHand);
    Vector3 handleWorld=new Vector3(sb.center.x, sb.min.y, sb.center.z);
    Vector3 fistCenter=fb.center;
    sword.position += (fistCenter - handleWorld);
    // sink slightly into the fist so it looks gripped
    sword.position += wh.transform.up*0.03f + wh.transform.forward*0.01f;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    var sb2=RB(sword);
    return "placed: swordHeight="+sb2.size.y.ToString("0.00")+" handle->fist";
  }
}
