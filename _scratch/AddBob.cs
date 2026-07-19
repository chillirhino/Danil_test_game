using UnityEngine;
using PoK.Player;
public class AddBob {
  public static string Main(){
    var body=GameObject.Find("FPBody");
    if(body.GetComponent<WalkBob>()==null){ var wb=body.AddComponent<WalkBob>(); wb.controller=GameObject.Find("Player").GetComponent<FirstPersonController>(); }
    // raise the body a touch so the gripping hand shows in frame
    body.transform.localPosition+=new Vector3(0f,0.07f,0f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "walkbob added, body raised";
  }
}
