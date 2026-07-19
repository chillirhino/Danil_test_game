using UnityEngine;
public class SwordAtHand {
  static Transform Find(Transform r,string n){ foreach(var t in r.GetComponentsInChildren<Transform>()) if(t.name==n) return t; return null; }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    Transform rHand=null,sword=null;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    if(sword==null){ // sword may be parented under bone now; search whole vm
      foreach(var t in wh.GetComponentsInChildren<Transform>()) if(t.name.ToLower().Contains("sword")) sword=t;
    }
    var handBone=Find(rHand,"Hand");
    sword.SetParent(wh.transform,true);
    float sc=0.30f; sword.localScale=Vector3.one*sc;
    sword.rotation=Quaternion.LookRotation(wh.forward, Vector3.up); // upright, facing forward
    sword.rotation=Quaternion.Euler(0f,0f,0f); // blade up (identity)
    sword.position=handBone.position + Vector3.up*(1.3f*sc) + wh.forward*0.02f;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "sword at hand bone world pos";
  }
}
