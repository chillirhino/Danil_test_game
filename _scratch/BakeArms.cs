using UnityEngine;
using UnityEditor;
using PoK.Player;
public class BakeArms {
  static string Path(string kw){ foreach(var g in AssetDatabase.FindAssets("t:Prefab")){var p=AssetDatabase.GUIDToAssetPath(g);if(p.Contains("Polytope")&&p.ToLower().Contains(kw))return p;}return null; }
  public static string Main(){
    // 1) widen the swing on the controller instance
    var vm=GameObject.Find("ViewModel");
    var ctrl=vm.GetComponent<ViewModelController>();
    var so=new SerializedObject(ctrl);
    so.FindProperty("swingPosOffset").vector3Value=new Vector3(-0.30f,-0.05f,0.12f);
    so.FindProperty("swingRotOffset").vector3Value=new Vector3(28f,-115f,-58f);
    so.FindProperty("swingDuration").floatValue=0.34f;
    so.ApplyModifiedProperties();

    // 2) bake the male gauntlet skinned mesh to a static mesh
    var gp=AssetDatabase.LoadAssetAtPath<GameObject>(Path("pt_male_armor_01_a_gauntlets"));
    if(gp==null) gp=AssetDatabase.LoadAssetAtPath<GameObject>(Path("male_armor_cloth_00_gauntlets"));
    if(gp==null) return "no gauntlet prefab";
    var inst=(GameObject)PrefabUtility.InstantiatePrefab(gp);
    var smr=inst.GetComponentInChildren<SkinnedMeshRenderer>();
    var baked=new Mesh(); smr.BakeMesh(baked);
    System.IO.Directory.CreateDirectory(Application.dataPath+"/Art/Models");
    AssetDatabase.CreateAsset(baked,"Assets/Art/Models/Gauntlet_baked.asset");
    var mat=smr.sharedMaterial;
    var bsz=baked.bounds.size; var bc=baked.bounds.center;
    Object.DestroyImmediate(inst);

    // 3) right arm under WeaponHand (swings with sword)
    var hand=GameObject.Find("WeaponHand");
    var old=hand.transform.Find("RightArm"); if(old!=null) Object.DestroyImmediate(old.gameObject);
    var arm=new GameObject("RightArm",typeof(MeshFilter),typeof(MeshRenderer));
    arm.transform.SetParent(hand.transform,false);
    arm.GetComponent<MeshFilter>().sharedMesh=baked;
    arm.GetComponent<MeshRenderer>().sharedMaterial=mat;
    // rough placement: scale to forearm ~0.35, put hand end near grip
    float sc=0.35f/Mathf.Max(0.01f,bsz.y);
    arm.transform.localScale=Vector3.one*sc;
    arm.transform.localPosition=new Vector3(0f,-0.1f,-0.05f);
    arm.transform.localRotation=Quaternion.Euler(-70f,0f,0f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "gauntlet baked bounds="+bsz+" center="+bc+" scale="+sc.ToString("0.000");
  }
}
