using UnityEngine;
public class CleanArms {
  public static string Main(){
    var vm=GameObject.Find("ViewModel");
    int removed=0;
    foreach(var hn in new[]{"WeaponHand","LeftHand"}){
      var h=vm.transform.Find(hn); if(h==null) continue;
      foreach(var pn in new[]{"Forearm","Cuff"}){
        var p=h.Find(pn); if(p!=null){Object.DestroyImmediate(p.gameObject);removed++;}
      }
    }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "removed "+removed+" primitive parts";
  }
}
