using UnityEngine;
public class UnRoll {
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody");
    foreach(var pair in new[]{("PT_RightForeArm","PT_RightHand"),("PT_LeftForeArm","PT_LeftHand")}){
      var fore=F(body.transform,pair.Item1); var hand=F(body.transform,pair.Item2);
      Vector3 axis=(hand.position-fore.position).normalized;
      fore.rotation=Quaternion.AngleAxis(-90f,axis)*fore.rotation;
    }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "reverted roll";
  }
}
