using UnityEngine;
public class AimArms2 {
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  static void Aim(Transform upper, Transform hand, Vector3 targetPos){
    Vector3 cur=hand.position-upper.position; Vector3 tgt=targetPos-upper.position;
    upper.rotation=Quaternion.FromToRotation(cur,tgt)*upper.rotation;
  }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var rUp=F(body.transform,"PT_RightArm"); var rHand=F(body.transform,"PT_RightHand");
    var lUp=F(body.transform,"PT_LeftArm"); var lHand=F(body.transform,"PT_LeftHand");
    Vector3 rT=cam.position+cam.forward*0.55f+cam.right*0.16f-cam.up*0.16f;
    Vector3 lT=cam.position+cam.forward*0.55f-cam.right*0.16f-cam.up*0.16f;
    Aim(rUp,rHand,rT); Aim(lUp,lHand,lT);
    // orient the sword blade up (it sits under the weapon slot)
    var slot=F(body.transform,"PT_Right_Hand_Weapon_slot");
    Transform sword=slot!=null&&slot.childCount>0?slot.GetChild(0):null;
    if(sword!=null) sword.localRotation=Quaternion.Euler(180f,0f,0f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "re-aimed higher, sword="+(sword!=null);
  }
}
