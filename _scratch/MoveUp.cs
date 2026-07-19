using UnityEngine;
public class MoveUp {
  public static string Main(){
    var p=GameObject.Find("Player");
    if(p==null) return "no player";
    p.transform.position=new Vector3(0f,10f,15f);
    p.transform.rotation=Quaternion.Euler(35f,180f,0f);
    var fpc=p.GetComponent<PoK.Player.FirstPersonController>();
    if(fpc!=null) fpc.autoWalk=false;
    return "moved up at "+p.transform.position;
  }
}
