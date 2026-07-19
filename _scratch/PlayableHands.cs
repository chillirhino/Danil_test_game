using UnityEngine;
using UnityEditor;
public class PlayableHands {
  public static string Main(){
    var vm=GameObject.Find("ViewModel");
    var left=vm.transform.Find("LeftHand");
    // remove torch parts (TorchWood cylinder, Flame sphere, TorchLight)
    var kill=new System.Collections.Generic.List<GameObject>();
    foreach(Transform c in left){
      if(c.name=="TorchLight"){kill.Add(c.gameObject);continue;}
      var r=c.GetComponent<Renderer>();
      var mn=r!=null&&r.sharedMaterial!=null?r.sharedMaterial.name:"";
      if(mn=="TorchWood"||mn=="Flame") kill.Add(c.gameObject);
    }
    foreach(var g in kill) Object.DestroyImmediate(g);

    // lower + scale hands for a natural playable FP pose
    var wh=vm.transform.Find("WeaponHand");
    wh.localPosition=new Vector3(0.16f,-0.24f,0.5f);
    wh.localRotation=Quaternion.Euler(0f,0f,10f);
    var rArm=wh.Find("RightArm");
    rArm.localScale=Vector3.one*0.30f;
    rArm.localPosition=new Vector3(0f,-0.08f,0f);
    rArm.localRotation=Quaternion.Euler(-10f,12f,90f);

    left.localPosition=new Vector3(-0.16f,-0.24f,0.5f);
    left.localRotation=Quaternion.Euler(0f,0f,-10f);
    var lArm=left.Find("LeftArm");
    lArm.localScale=Vector3.one*0.30f;
    lArm.localPosition=new Vector3(0f,-0.08f,0f);
    lArm.localRotation=Quaternion.Euler(-10f,-12f,90f);

    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "torch removed, hands lowered/scaled";
  }
}
