using UnityEngine;
public class AdjustHands {
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  static void Aim(Transform up, Transform h, Vector3 t){ Vector3 c=h.position-up.position; up.rotation=Quaternion.FromToRotation(c,t-up.position)*up.rotation; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var rUp=F(body.transform,"PT_RightArm"); var rHand=F(body.transform,"PT_RightHand");
    var lUp=F(body.transform,"PT_LeftArm"); var lHand=F(body.transform,"PT_LeftHand");
    Vector3 rT=cam.position+cam.forward*0.48f+cam.right*0.14f-cam.up*0.12f;
    Vector3 lT=cam.position+cam.forward*0.48f-cam.right*0.14f-cam.up*0.12f;
    Aim(rUp,rHand,rT); Aim(lUp,lHand,lT);
    var slot=F(body.transform,"PT_Right_Hand_Weapon_slot");
    Transform sword=slot!=null&&slot.childCount>0?slot.GetChild(0):null;
    if(sword!=null){ Vector3 want=(cam.up*0.9f+cam.forward*0.2f).normalized; sword.rotation=Quaternion.FromToRotation(sword.up,want)*sword.rotation; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "hands adjusted";
  }
}
