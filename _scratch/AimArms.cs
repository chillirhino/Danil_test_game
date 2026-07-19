using UnityEngine;
public class AimArms {
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  static void Aim(Transform upper, Transform hand, Vector3 targetPos){
    Vector3 cur=hand.position-upper.position;
    Vector3 tgt=targetPos-upper.position;
    upper.rotation=Quaternion.FromToRotation(cur,tgt)*upper.rotation;
  }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    if(body==null) return "no body";
    var rUp=F(body.transform,"PT_RightArm"); var rHand=F(body.transform,"PT_RightHand");
    var lUp=F(body.transform,"PT_LeftArm"); var lHand=F(body.transform,"PT_LeftHand");
    Vector3 rT=cam.position+cam.forward*0.5f+cam.right*0.16f-cam.up*0.28f;
    Vector3 lT=cam.position+cam.forward*0.5f-cam.right*0.16f-cam.up*0.28f;
    Aim(rUp,rHand,rT);
    Aim(lUp,lHand,lT);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "arms aimed forward";
  }
}
