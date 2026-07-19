using UnityEngine;
public class RestoreVM {
  public static string Main(){
    var hand=GameObject.Find("WeaponHand");
    if(hand==null) return "no hand";
    hand.transform.localPosition=new Vector3(0.22f,-0.35f,0.55f);
    hand.transform.localRotation=Quaternion.Euler(0f,0f,15f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "restored idle pose";
  }
}
