using UnityEngine;
public class PlaceSword {
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    if(wh==null) return "no WeaponHand";
    wh.transform.localRotation=Quaternion.Euler(-25f,0f,-8f);
    Transform sword=null,rhand=null;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("sword"))sword=c; if(n.Contains("right"))rhand=c; }
    if(sword!=null){ sword.localScale=Vector3.one*0.22f; sword.localPosition=new Vector3(0.0f,0.13f,0.05f); sword.localRotation=Quaternion.Euler(25f,0f,8f); }
    // slight grip tilt on the hand mesh
    if(rhand!=null) rhand.localRotation=Quaternion.Euler(-20f,0f,10f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "sword placed in hand";
  }
}
