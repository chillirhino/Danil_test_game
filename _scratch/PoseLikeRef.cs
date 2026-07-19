using UnityEngine;
public class PoseLikeRef {
  static Bounds RB(Transform t){ Bounds b=new Bounds(t.position,Vector3.zero); bool f=false; foreach(var r in t.GetComponentsInChildren<Renderer>()){ if(!f){b=r.bounds;f=true;} else b.Encapsulate(r.bounds);} return b; }
  static void Anchor(Transform item, Transform fist, Transform vm, float up){
    Bounds ib=RB(item), fb=RB(fist);
    item.position += (fb.center - new Vector3(ib.center.x, ib.min.y, ib.center.z)) + vm.up*up;
  }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand").transform;
    var lh=GameObject.Find("LeftHand").transform;
    // corners, raised so fists are visible, modest inward forearm tilt
    wh.localPosition=new Vector3(0.25f,-0.22f,0.46f); wh.localRotation=Quaternion.Euler(0f,0f,10f);
    lh.localPosition=new Vector3(-0.25f,-0.22f,0.46f); lh.localRotation=Quaternion.Euler(0f,0f,-10f);
    // find items + fists
    Transform sword=null,rFist=null;
    foreach(Transform c in wh){ var n=c.name.ToLower(); if(n.Contains("sword"))sword=c; if(n.Contains("right"))rFist=c; }
    var torch=lh.Find("Torch"); var lFist=lh.GetChild(0);
    // sword leans up-right; torch leans up-left
    if(sword!=null){ sword.localScale=Vector3.one*0.62f; sword.localRotation=Quaternion.Euler(12f,0f,16f); Anchor(sword,rFist,wh,0.03f); }
    if(torch!=null){ torch.localRotation=Quaternion.Euler(-8f,0f,-14f); Anchor(torch,lFist,lh,0.02f); }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "posed like ref: fists in corners, sword right, torch left";
  }
}
