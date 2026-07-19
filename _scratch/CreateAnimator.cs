using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
public class CreateAnimator {
  static AnimationClip Clip(string path){
    foreach(var a in AssetDatabase.LoadAllAssetsAtPath(path)) if(a is AnimationClip ac && !ac.name.StartsWith("__")) return ac;
    return null;
  }
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var idle=Clip("Assets/Sword And Shield Idle.fbx");
    var slash=Clip("Assets/Stable Sword Inward Slash.fbx");
    if(idle==null||slash==null) return "clips missing i="+(idle!=null)+" s="+(slash!=null);
    const string cpath="Assets/Art/FPCombat.controller";
    var ac=AnimatorController.CreateAnimatorControllerAtPath(cpath);
    ac.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
    var sm=ac.layers[0].stateMachine;
    var sIdle=sm.AddState("Idle"); sIdle.motion=idle;
    var sAtk=sm.AddState("Attack"); sAtk.motion=slash;
    sm.defaultState=sIdle;
    var t1=sIdle.AddTransition(sAtk); t1.hasExitTime=false; t1.duration=0.06f; t1.AddCondition(AnimatorConditionMode.If,0,"Attack");
    var t2=sAtk.AddTransition(sIdle); t2.hasExitTime=true; t2.exitTime=0.8f; t2.duration=0.15f;

    var body=GameObject.Find("FPBody");
    var an=body.GetComponentInChildren<Animator>(true);
    an.runtimeAnimatorController=ac;
    an.applyRootMotion=false;
    an.enabled=true;

    // attach sword to right hand weapon slot
    var slot=F(body.transform,"PT_Right_Hand_Weapon_slot");
    string sw="no slot";
    if(slot!=null){
      // remove any existing sword
      for(int i=slot.childCount-1;i>=0;i--) Object.DestroyImmediate(slot.GetChild(i).gameObject);
      var sp=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PT_Sword_01_a t:Prefab")[0]));
      var s=(GameObject)PrefabUtility.InstantiatePrefab(sp,slot);
      s.transform.localPosition=Vector3.zero; s.transform.localRotation=Quaternion.identity; s.transform.localScale=Vector3.one;
      sw="sword attached";
    }
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "animator created; idle="+idle.name+" slash len="+slash.length.ToString("0.00")+"; "+sw;
  }
}
