using UnityEngine;
using UnityEditor;
using PoK.Player;
public class SetupVM {
  static string P(string kw){ foreach(var g in AssetDatabase.FindAssets("t:Prefab")){var p=AssetDatabase.GUIDToAssetPath(g);if(p.Contains("Polytope")&&p.ToLower().Contains(kw))return p;}return null; }
  public static string Main(){
    var cam=GameObject.Find("Main Camera");
    var old=GameObject.Find("ViewModel"); if(old!=null) Object.DestroyImmediate(old);
    var vm=new GameObject("ViewModel"); vm.transform.SetParent(cam.transform,false);
    var hand=new GameObject("WeaponHand"); hand.transform.SetParent(vm.transform,false);
    hand.transform.localPosition=new Vector3(0.22f,-0.35f,0.55f);
    hand.transform.localRotation=Quaternion.Euler(0f,0f,15f);
    var swordPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(P("sword_01_a"));
    var sword=(GameObject)PrefabUtility.InstantiatePrefab(swordPrefab,hand.transform);
    float scale=0.32f;
    sword.transform.localScale=Vector3.one*scale;
    sword.transform.localRotation=Quaternion.identity;
    // move handle (bottom of mesh) to hand origin: mesh bottom ~ -1.3 in sword space
    sword.transform.localPosition=new Vector3(0f,1.3f*scale,0f);
    var ctrl=vm.AddComponent<ViewModelController>();
    var so=new SerializedObject(ctrl);
    so.FindProperty("weaponHand").objectReferenceValue=hand.transform;
    var pl=GameObject.Find("Player");
    so.FindProperty("controller").objectReferenceValue=pl!=null?pl.GetComponent<FirstPersonController>():null;
    so.ApplyModifiedProperties();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "viewmodel set up";
  }
}
