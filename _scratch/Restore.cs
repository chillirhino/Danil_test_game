using UnityEngine;
public class Restore {
  public static string Main(){
    var p=GameObject.Find("Player");
    if(p==null) return "no player";
    p.transform.position=new Vector3(0f,1f,-8f);
    p.transform.rotation=Quaternion.identity;
    var fpc=p.GetComponent<PoK.Player.FirstPersonController>();
    if(fpc!=null) fpc.autoWalk=true;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "restored";
  }
}
