using System.Text;
using UnityEngine;
public class GripHands {
  static void Curl(Transform root, float angle){
    string[] fk={"Index","Middle","Ring","Pinky","Thumb"};
    foreach(var t in root.GetComponentsInChildren<Transform>()){
      foreach(var k in fk){ if(t.name.Contains(k)){ t.localRotation*=Quaternion.Euler(angle,0f,0f); break; } }
    }
  }
  public static string Main(){
    var wh=GameObject.Find("WeaponHand");
    Transform rHand=null,sword=null,lhNode=GameObject.Find("LeftHand").transform;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    var lHandMesh=lhNode.GetChild(0);
    // curl fingers into a grip
    Curl(rHand, 55f);
    Curl(lHandMesh, 60f);
    // sword handle into the right palm, blade up
    if(sword!=null){ float sc=0.30f; sword.localScale=Vector3.one*sc; sword.localPosition=new Vector3(0.02f,1.3f*sc,0.04f); sword.localRotation=Quaternion.Euler(4f,0f,-5f); }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "fingers curled + sword in palm";
  }
}
