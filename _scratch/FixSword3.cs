using UnityEngine;
public class FixSword3 {
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    Transform sword=null; foreach(Transform c in wh.transform){ if(c.name.ToLower().Contains("sword"))sword=c; }
    if(sword==null) return "no sword";
    float sc=0.30f; sword.localScale=Vector3.one*sc;
    sword.localPosition=new Vector3(0.0f, 1.3f*sc, 0.03f); // handle at hand, blade rises up
    sword.localRotation=Quaternion.Euler(6f,0f,-6f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "sword handle at hand, blade up";
  }
}
