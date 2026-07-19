using UnityEngine;
public class PoseArms {
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); if(body==null) return "no FPBody";
    var rArm=F(body.transform,"PT_RightArm");
    var rFore=F(body.transform,"PT_RightForeArm");
    var lArm=F(body.transform,"PT_LeftArm");
    var lFore=F(body.transform,"PT_LeftForeArm");
    if(rArm==null) return "no arm bone";
    rArm.localRotation*=Quaternion.Euler(0f,0f,70f);
    rFore.localRotation*=Quaternion.Euler(0f,50f,0f);
    lArm.localRotation*=Quaternion.Euler(0f,0f,-70f);
    lFore.localRotation*=Quaternion.Euler(0f,-50f,0f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "posed arms (test1)";
  }
}
