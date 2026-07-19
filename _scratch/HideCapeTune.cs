using UnityEngine;
using PoK.Player;
public class HideCapeTune {
  public static string Main(){
    var body=GameObject.Find("FPBody");
    int hid=0;
    foreach(var smr in body.GetComponentsInChildren<SkinnedMeshRenderer>(true))
      if(smr.name.ToLower().Contains("cape")){ smr.gameObject.SetActive(false); hid++; }
    var a=body.GetComponent<ArmsInView>();
    a.forward=0.5f; a.side=0.17f; a.down=0.15f;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "cape hidden="+hid+", arms tuned";
  }
}
