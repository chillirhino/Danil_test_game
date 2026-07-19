using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
public class FreezeFrame {
  static AnimationClip Clip(string p){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(p)) if(a is AnimationClip ac&&!ac.name.StartsWith("__")) return ac; return null; }
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    // freeze idle states (speed 0) so the sword is static while standing; walk-bob + slash still move
    var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Art/FPCombat.controller");
    foreach(var layer in ac.layers){
      foreach(var st in layer.stateMachine.states){
        if(st.state.name=="Idle"||st.state.name=="ArmIdle") st.state.speed=0f;
      }
    }
    EditorUtility.SetDirty(ac); AssetDatabase.SaveAssets();

    // reframe so the right hand is visible in the lower view
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var an=body.GetComponentInChildren<Animator>(true); an.enabled=false;
    var idle=Clip("Assets/Sword And Shield Idle.fbx"); idle.SampleAnimation(body,0f);
    var wb=body.GetComponent<PoK.Player.WalkBob>();
    var rHand=F(body.transform,"PT_RightHand");
    Vector3 target=cam.position+cam.forward*0.5f+cam.right*0.05f-cam.up*0.13f;
    body.transform.position+=target-rHand.position;
    an.enabled=true;
    // update walkbob base pos to new position
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "idle frozen + reframed by hand";
  }
}
