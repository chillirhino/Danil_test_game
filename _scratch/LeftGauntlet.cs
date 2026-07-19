using UnityEngine;
using UnityEditor;
public class LeftGauntlet {
  public static string Main(){
    var vm=GameObject.Find("ViewModel");
    var left=vm.transform.Find("LeftHand");
    if(left==null) return "no LeftHand";
    // remove primitive arm parts (cylinder forearm + sphere fist), keep torch handle/flame/light
    var toKill=new System.Collections.Generic.List<GameObject>();
    foreach(Transform c in left){
      var mf=c.GetComponent<MeshFilter>();
      bool isTorch=c.name=="TorchLight";
      // torch handle = cylinder with TorchWood, flame = sphere with Flame; keep those. arm = leather cyl + skin sphere
      var r=c.GetComponent<Renderer>();
      string matName=r!=null&&r.sharedMaterial!=null?r.sharedMaterial.name:"";
      if(matName=="Leather"||matName=="Skin") toKill.Add(c.gameObject);
    }
    foreach(var g in toKill) Object.DestroyImmediate(g);
    // add gauntlet mesh gripping the torch
    var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Models/Gauntlet_R.asset");
    var srcArm=GameObject.Find("WeaponHand").transform.Find("RightArm");
    var mat=srcArm.GetComponent<MeshRenderer>().sharedMaterial;
    var arm=new GameObject("LeftArm",typeof(MeshFilter),typeof(MeshRenderer));
    arm.transform.SetParent(left,false);
    arm.GetComponent<MeshFilter>().sharedMesh=mesh;
    arm.GetComponent<MeshRenderer>().sharedMaterial=mat;
    arm.transform.localScale=Vector3.one*0.22f;
    arm.transform.localPosition=new Vector3(0f,-0.02f,0f);
    arm.transform.localRotation=Quaternion.Euler(0f,-12f,90f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "left gauntlet placed";
  }
}
