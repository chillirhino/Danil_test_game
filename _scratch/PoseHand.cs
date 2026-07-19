using UnityEngine;
using UnityEditor;
public class PoseHand {
  public static string Main(){
    // tint armor material to steel so it's not white
    var am=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Materials/PT_Armors_Material.mat");
    if(am!=null){ am.SetColor("_BaseColor",new Color(0.5f,0.52f,0.58f)); if(am.HasProperty("_Smoothness"))am.SetFloat("_Smoothness",0.3f); EditorUtility.SetDirty(am); }
    var arm=GameObject.Find("WeaponHand").transform.Find("RightArm");
    if(arm==null) return "no arm";
    arm.localRotation=Quaternion.Euler(0f,0f,90f);
    arm.localPosition=new Vector3(0f,-0.15f,0f);
    AssetDatabase.SaveAssets();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "posed rot(0,0,90)";
  }
}
