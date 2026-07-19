using UnityEngine;
using UnityEditor;
public class InstChar {
  public static string Main(){
    var old=GameObject.Find("CharTest"); if(old!=null) Object.DestroyImmediate(old);
    var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_Armors/PT_Lowpoly_Armors_Male_Moduar_Free.prefab");
    var c=(GameObject)PrefabUtility.InstantiatePrefab(p);
    c.name="CharTest";
    c.transform.position=new Vector3(0f,0f,-4.5f);
    c.transform.rotation=Quaternion.Euler(0f,180f,0f); // face the camera (camera looks +Z)
    // enable only Armor_01_A set, disable the rest
    foreach(var smr in c.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
      bool keep=smr.name.Contains("Armor_01_A");
      smr.gameObject.SetActive(keep);
    }
    // apply pose clip
    string clipInfo="no clip";
    foreach(var a in AssetDatabase.LoadAllAssetsAtPath("Assets/Polytope Studio/Lowpoly_Characters/Animations/PT_Pose_01.fbx"))
      if(a is AnimationClip ac && !ac.name.StartsWith("__")){ ac.SampleAnimation(c, ac.length); clipInfo="sampled "+ac.name; }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return clipInfo;
  }
}
