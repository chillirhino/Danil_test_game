using UnityEngine;
public class PoseCorners {
  public static string Main(){
    var wh=GameObject.Find("WeaponHand").transform;
    var lh=GameObject.Find("LeftHand").transform;
    // spread to bottom corners + angle forearms inward (diagonal from corners)
    wh.localPosition=new Vector3(0.26f,-0.32f,0.46f);
    wh.localRotation=Quaternion.Euler(0f,0f,28f);
    lh.localPosition=new Vector3(-0.26f,-0.32f,0.46f);
    lh.localRotation=Quaternion.Euler(0f,0f,-28f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "hands to corners, angled inward";
  }
}
