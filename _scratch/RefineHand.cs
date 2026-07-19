using UnityEngine;
public class RefineHand {
  public static string Main(){
    var hand=GameObject.Find("WeaponHand");
    hand.transform.localPosition=new Vector3(0.17f,-0.10f,0.5f);
    var arm=hand.transform.Find("RightArm");
    if(arm==null) return "no arm";
    arm.localScale=Vector3.one*0.22f;
    arm.localPosition=new Vector3(0.0f,-0.05f,0.0f);
    arm.localRotation=Quaternion.Euler(0f,12f,90f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "refined";
  }
}
