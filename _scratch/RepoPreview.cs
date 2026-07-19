using UnityEngine;
using UnityEditor;
public class RepoPreview {
  static AnimationClip Clip(string p){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(p)) if(a is AnimationClip ac&&!ac.name.StartsWith("__")) return ac; return null; }
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var an=body.GetComponentInChildren<Animator>(true); an.enabled=false;
    var idle=Clip("Assets/Sword And Shield Idle.fbx");
    idle.SampleAnimation(body,0.5f);
    var rHand=F(body.transform,"PT_RightHand");
    // push body further from camera so slash sweeps in front, not through the face
    Vector3 target=cam.position+cam.forward*0.62f+cam.right*0.06f-cam.up*0.22f;
    body.transform.position+=target-rHand.position;
    // now preview slash
    var slash=Clip("Assets/Stable Sword Inward Slash.fbx");
    slash.SampleAnimation(body, slash.length*0.45f);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "repositioned further, slash preview";
  }
}
