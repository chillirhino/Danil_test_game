using UnityEngine;
public class RemoveVM {
  public static string Main(){
    var vm=GameObject.Find("ViewModel");
    if(vm!=null) Object.DestroyImmediate(vm);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return vm!=null?"weapon/viewmodel removed":"nothing to remove";
  }
}
