using UnityEngine;
using UnityEditor;
using PoK.Player;
public class SetupHands {
  public static string Main(){
    var cam=GameObject.Find("Main Camera");
    var fp=GameObject.Find("FPBody"); if(fp!=null) fp.SetActive(false);
    var oldvm=GameObject.Find("HandVM"); if(oldvm!=null) Object.DestroyImmediate(oldvm);
    var hand=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleHands/Prefabs/WhiteHand.prefab");
    var swordPf=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PT_Sword_01_a t:Prefab")[0]));

    var vm=new GameObject("HandVM"); vm.transform.SetParent(cam.transform,false);
    var wh=new GameObject("WeaponHand"); wh.transform.SetParent(vm.transform,false);
    wh.transform.localPosition=new Vector3(0.17f,-0.28f,0.5f); wh.transform.localRotation=Quaternion.Euler(0,0,0);
    var rHand=(GameObject)PrefabUtility.InstantiatePrefab(hand,wh.transform);
    rHand.name="RightHand"; rHand.transform.localScale=Vector3.one*0.25f; rHand.transform.localRotation=Quaternion.identity;
    var sword=(GameObject)PrefabUtility.InstantiatePrefab(swordPf,wh.transform);
    sword.transform.localScale=Vector3.one*0.22f; sword.transform.localPosition=new Vector3(0,0.28f,0.05f); sword.transform.localRotation=Quaternion.identity;

    var lh=new GameObject("LeftHand"); lh.transform.SetParent(vm.transform,false);
    lh.transform.localPosition=new Vector3(-0.17f,-0.28f,0.5f);
    var lHand=(GameObject)PrefabUtility.InstantiatePrefab(hand,lh.transform);
    lHand.name="LeftHandMesh"; lHand.transform.localScale=new Vector3(-0.25f,0.25f,0.25f); lHand.transform.localRotation=Quaternion.identity;

    var fpc=GameObject.Find("Player").GetComponent<FirstPersonController>();
    var ctrl=vm.AddComponent<ViewModelController>(); var so=new SerializedObject(ctrl);
    so.FindProperty("weaponHand").objectReferenceValue=wh.transform; so.FindProperty("controller").objectReferenceValue=fpc; so.ApplyModifiedProperties();
    var wb=vm.AddComponent<WalkBob>(); wb.controller=fpc;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "hand viewmodel set up";
  }
}
