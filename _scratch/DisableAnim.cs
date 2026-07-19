using UnityEngine;
public class DisableAnim {
  public static string Main(){
    var body=GameObject.Find("FPBody");
    var an=body.GetComponentInChildren<Animator>();
    if(an!=null){ an.enabled=false; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return an!=null?"animator disabled":"no animator";
  }
}
