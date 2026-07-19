using UnityEngine;
using UnityEditor;
public class ResetHands {
  public static string Main(){
    var hand=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleHands/Prefabs/WhiteHand.prefab");
    var wh=GameObject.Find("WeaponHand"); var lh=GameObject.Find("LeftHand").transform;
    // remove old hand meshes (keep sword)
    Transform sword=null;
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("sword"))sword=c; }
    for(int i=wh.transform.childCount-1;i>=0;i--){ var c=wh.transform.GetChild(i); if(!c.name.ToLower().Contains("sword")) Object.DestroyImmediate(c.gameObject); }
    for(int i=lh.childCount-1;i>=0;i--) Object.DestroyImmediate(lh.GetChild(i).gameObject);
    // fresh right + left (bind pose, identity)
    var r=(GameObject)PrefabUtility.InstantiatePrefab(hand,wh.transform); r.name="RightHand"; r.transform.localScale=Vector3.one*0.25f; r.transform.localRotation=Quaternion.identity; r.transform.localPosition=Vector3.zero;
    var l=(GameObject)PrefabUtility.InstantiatePrefab(hand,lh); l.name="LeftHandMesh"; l.transform.localScale=new Vector3(-0.25f,0.25f,0.25f); l.transform.localRotation=Quaternion.identity; l.transform.localPosition=Vector3.zero;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "hands reset to bind pose";
  }
}
