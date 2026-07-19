using UnityEngine;
using PoK.Player;
public class TuneAIV {
  public static string Main(){
    var a=GameObject.Find("FPBody").GetComponent<ArmsInView>();
    a.forward=0.36f; a.side=0.13f; a.down=0.05f;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "tuned f=0.36 s=0.13 d=0.05";
  }
}
