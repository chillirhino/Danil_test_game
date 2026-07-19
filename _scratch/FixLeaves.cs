using UnityEngine;
using UnityEditor;
public class FixLeaves {
  public static string Main(){
    var green=new Color(0.42f,0.55f,0.25f);
    foreach(var n in new[]{"branch01","branch02","bush01","bush02"}){
      var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/"+n+".mat");
      if(m==null) continue;
      if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", green);
      EditorUtility.SetDirty(m);
    }
    AssetDatabase.SaveAssets();
    // level the camera
    var cam=GameObject.Find("Main Camera");
    if(cam!=null) cam.transform.localRotation=Quaternion.identity;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "leaves tinted, camera leveled";
  }
}
