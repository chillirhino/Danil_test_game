using UnityEngine;
public class AimArms3 {
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  static void Aim(Transform upper, Transform hand, Vector3 t){ Vector3 c=hand.position-upper.position; upper.rotation=Quaternion.FromToRotation(c,t-upper.position)*upper.rotation; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var rUp=F(body.transform,"PT_RightArm"); var rHand=F(body.transform,"PT_RightHand");
    var lUp=F(body.transform,"PT_LeftArm"); var lHand=F(body.transform,"PT_LeftHand");
    Vector3 rT=cam.position+cam.forward*0.52f+cam.right*0.15f-cam.up*0.10f;
    Vector3 lT=cam.position+cam.forward*0.52f-cam.right*0.15f-cam.up*0.10f;
    Aim(rUp,rHand,rT); Aim(lUp,lHand,lT);
    // bend elbows a bit (natural)
    var rFore=F(body.transform,"PT_RightForeArm"); var lFore=F(body.transform,"PT_LeftForeArm");
    // sword blade up: point the sword's up-axis along camera up
    var slot=F(body.transform,"PT_Right_Hand_Weapon_slot");
    Transform sword=slot!=null&&slot.childCount>0?slot.GetChild(0):null;
    if(sword!=null){
      // sword local +Y is blade dir (tip). Make blade point up-forward in world.
      Vector3 want=(cam.up*0.85f+cam.forward*0.3f).normalized;
      sword.rotation=Quaternion.FromToRotation(sword.up, want)*sword.rotation;
    }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "aim3 done sword="+(sword!=null);
  }
}
