using UnityEngine;
using UnityEditor;
public class CharFP {
  static Transform Find(Transform root, string name){
    if(root.name==name) return root;
    foreach(Transform c in root){ var r=Find(c,name); if(r!=null) return r; }
    return null;
  }
  public static string Main(){
    // remove old primitive viewmodel
    var oldvm=GameObject.Find("ViewModel"); if(oldvm!=null) Object.DestroyImmediate(oldvm);
    var c=GameObject.Find("CharTest"); if(c==null) return "no CharTest";
    c.name="FPBody";
    var cam=GameObject.Find("Main Camera");
    // parent to camera, face forward (+Z like camera)
    c.transform.SetParent(cam.transform,false);
    c.transform.localRotation=Quaternion.identity;
    // align head with camera
    var head=Find(c.transform,"PT_Head");
    Vector3 headLocal=cam.transform.InverseTransformPoint(head.position);
    c.transform.localPosition-=new Vector3(headLocal.x, headLocal.y-0.02f, headLocal.z-0.08f);
    // hide head bits
    foreach(var smr in c.GetComponentsInChildren<SkinnedMeshRenderer>(true))
      if(smr.name.Contains("helmet")||smr.name.Contains("head")||smr.name.Contains("hair")||smr.name.Contains("beard")) smr.gameObject.SetActive(false);
    // attach sword to right hand weapon slot
    var slot=Find(c.transform,"PT_Right_Hand_Weapon_slot");
    string sInfo="no slot";
    if(slot!=null){
      var sp=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PT_Sword_01_a t:Prefab")[0]));
      var sw=(GameObject)PrefabUtility.InstantiatePrefab(sp,slot);
      sw.transform.localPosition=Vector3.zero; sw.transform.localRotation=Quaternion.identity; sw.transform.localScale=Vector3.one;
      sInfo="sword attached";
    }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "FP body set; headLocal="+headLocal.ToString("0.00")+"; "+sInfo;
  }
}
