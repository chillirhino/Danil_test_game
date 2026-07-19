using UnityEngine;
public class RestoreSword {
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    Transform sword=null;
    foreach(var t in wh.GetComponentsInChildren<Transform>(true)) if(t.name.ToLower().Contains("sword")){ sword=t; break; }
    if(sword==null) return "sword lost";
    sword.SetParent(wh.transform,false);
    float sc=0.30f; sword.localScale=Vector3.one*sc;
    sword.localPosition=new Vector3(0.03f, 1.3f*sc, 0.04f);
    sword.localRotation=Quaternion.identity;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "sword restored (visible, blade up)";
  }
}
