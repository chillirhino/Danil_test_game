using UnityEngine;
using PoK.Player;
public class AddAIV {
  public static string Main(){
    var body=GameObject.Find("FPBody");
    if(body.GetComponent<ArmsInView>()==null){ var a=body.AddComponent<ArmsInView>(); a.cameraTransform=GameObject.Find("Main Camera").transform; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "ArmsInView added";
  }
}
