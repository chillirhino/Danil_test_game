using UnityEngine;
public class RigLower {
  public static string Main(){
    var body=GameObject.Find("FPBody");
    var an=body.GetComponentInChildren<Animator>(true);
    string rig=an!=null&&an.avatar!=null?("avatar="+an.avatar.name+" isHuman="+an.avatar.isHuman):"no avatar";
    // lower the body so arms grow from the bottom of the screen
    body.transform.localPosition+=new Vector3(0f,-0.18f,0f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "lowered; "+rig;
  }
}
