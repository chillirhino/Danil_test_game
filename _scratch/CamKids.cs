using System.Text;
using UnityEngine;
public class CamKids {
  public static string Main(){
    var cam=GameObject.Find("Main Camera");
    if(cam==null) return "no cam";
    var sb=new StringBuilder("Main Camera children: ");
    foreach(Transform c in cam.transform) sb.Append(c.name+"(active="+c.gameObject.activeSelf+") ");
    if(cam.transform.childCount==0) sb.Append("(none)");
    // also any stray Polytope instances in scene
    sb.Append("\nScene roots: ");
    foreach(var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) sb.Append(g.name+" ");
    return sb.ToString();
  }
}
