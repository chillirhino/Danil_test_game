using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
public class MaskedAttack {
  static AnimationClip Clip(string p){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(p)) if(a is AnimationClip ac&&!ac.name.StartsWith("__")) return ac; return null; }
  public static string Main(){
    var idle=Clip("Assets/Sword And Shield Idle.fbx");
    var slash=Clip("Assets/Stable Sword Inward Slash.fbx");
    // right-arm-only mask
    var mask=new AvatarMask();
    foreach(AvatarMaskBodyPart p in System.Enum.GetValues(typeof(AvatarMaskBodyPart))){
      if(p==AvatarMaskBodyPart.LastBodyPart) continue;
      bool on=(p==AvatarMaskBodyPart.RightArm||p==AvatarMaskBodyPart.RightFingers);
      mask.SetHumanoidBodyPartActive(p,on);
    }
    AssetDatabase.CreateAsset(mask,"Assets/Art/RightArmMask.mask");

    const string cpath="Assets/Art/FPCombat.controller";
    AssetDatabase.DeleteAsset(cpath);
    var ac=AnimatorController.CreateAnimatorControllerAtPath(cpath);
    ac.AddParameter("Attack",AnimatorControllerParameterType.Trigger);
    // base layer: idle full body
    var baseSm=ac.layers[0].stateMachine;
    var bIdle=baseSm.AddState("Idle"); bIdle.motion=idle; baseSm.defaultState=bIdle;
    // attack layer: mask=right arm, Idle default + Slash
    ac.AddLayer("Attack");
    var layers=ac.layers;
    layers[1].avatarMask=mask; layers[1].defaultWeight=1f; layers[1].blendingMode=AnimatorLayerBlendingMode.Override;
    ac.layers=layers;
    var atkSm=ac.layers[1].stateMachine;
    var aIdle=atkSm.AddState("ArmIdle"); aIdle.motion=idle; atkSm.defaultState=aIdle;
    var aSlash=atkSm.AddState("Slash"); aSlash.motion=slash;
    var t1=aIdle.AddTransition(aSlash); t1.hasExitTime=false; t1.duration=0.05f; t1.AddCondition(AnimatorConditionMode.If,0,"Attack");
    var t2=aSlash.AddTransition(aIdle); t2.hasExitTime=true; t2.exitTime=0.75f; t2.duration=0.15f;

    var body=GameObject.Find("FPBody");
    var an=body.GetComponentInChildren<Animator>(true);
    an.runtimeAnimatorController=ac; an.applyRootMotion=false; an.enabled=true;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "masked attack layer created";
  }
}
