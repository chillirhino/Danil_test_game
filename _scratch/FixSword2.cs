using UnityEngine;
public class FixSword2 {
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    wh.transform.localRotation=Quaternion.identity;
    Transform sword=null,rhand=null;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("sword"))sword=c; if(n.Contains("right"))rhand=c; }
    if(rhand!=null) rhand.localRotation=Quaternion.Euler(-15f,0f,0f);
    if(sword!=null){ sword.localScale=Vector3.one*0.30f; sword.localPosition=new Vector3(0.02f,0.16f,0.02f); sword.localRotation=Quaternion.identity; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "sword vertical in hand (identity)";
  }
}
