using UnityEngine;
using UnityEditor;
public class RealHand {
  static string FindMaleGauntlet(){
    foreach(var g in AssetDatabase.FindAssets("t:Prefab")){
      var p=AssetDatabase.GUIDToAssetPath(g); var lp=p.ToLower();
      if(p.Contains("Polytope")&&lp.Contains("gauntlets")&&lp.Contains("male")&&!lp.Contains("female")) return p;
    }
    return null;
  }
  public static string Main(){
    var gpPath=FindMaleGauntlet();
    if(gpPath==null) return "no male gauntlet";
    var gp=AssetDatabase.LoadAssetAtPath<GameObject>(gpPath);
    var inst=(GameObject)PrefabUtility.InstantiatePrefab(gp);
    var smr=inst.GetComponentInChildren<SkinnedMeshRenderer>();
    var baked=new Mesh(); smr.BakeMesh(baked);
    // recenter to origin
    var verts=baked.vertices; var c=baked.bounds.center;
    for(int i=0;i<verts.Length;i++) verts[i]-=c;
    baked.vertices=verts; baked.RecalculateBounds();
    AssetDatabase.CreateAsset(baked,"Assets/Art/Models/Gauntlet_R.asset");
    var mat=smr.sharedMaterial;
    Object.DestroyImmediate(inst);

    var hand=GameObject.Find("WeaponHand");
    var old=hand.transform.Find("RightArm"); if(old!=null) Object.DestroyImmediate(old.gameObject);
    var arm=new GameObject("RightArm",typeof(MeshFilter),typeof(MeshRenderer));
    arm.transform.SetParent(hand.transform,false);
    arm.GetComponent<MeshFilter>().sharedMesh=baked;
    arm.GetComponent<MeshRenderer>().sharedMaterial=mat;
    float s=0.35f/baked.bounds.size.x; // forearm length along X
    arm.transform.localScale=Vector3.one*s;
    arm.transform.localPosition=Vector3.zero;
    arm.transform.localRotation=Quaternion.identity;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "gauntlet placed, recentered size="+baked.bounds.size+" scale="+s.ToString("0.000");
  }
}
