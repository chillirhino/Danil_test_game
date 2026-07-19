using UnityEngine;
public class TunePose {
  public static string Main(){
    var hand=GameObject.Find("WeaponHand");
    if(hand==null) return "no hand";
    hand.transform.localPosition=new Vector3(0.17f,-0.16f,0.5f);
    hand.transform.localRotation=Quaternion.Euler(0f,0f,12f);
    // shrink sword
    Transform sword=null; foreach(Transform c in hand.transform){ if(c.name.ToLower().Contains("sword")) sword=c; }
    if(sword!=null){ float sc=0.26f; sword.localScale=Vector3.one*sc; sword.localPosition=new Vector3(0f,1.3f*sc,0f);}    
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "pose tuned; sword="+(sword!=null);
  }
}
