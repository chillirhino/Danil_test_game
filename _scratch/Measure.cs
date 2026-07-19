using System.Text;
using UnityEngine;
public class Measure {
  static Transform F(Transform r,string n){ foreach(var t in r.GetComponentsInChildren<Transform>(true)) if(t.name==n) return t; return null; }
  static Vector3 Center(Transform t){ var r=t.GetComponentInChildren<Renderer>(); return r!=null?r.bounds.center:t.position; }
  public static string Main(){
    var cam=GameObject.Find("Main Camera").transform;
    var wh=GameObject.Find("WeaponHand");
    Transform rHand=null,sword=null,lh=GameObject.Find("LeftHand").transform.GetChild(0);
    foreach(Transform c in wh.transform){ var n=c.name.ToLower(); if(n.Contains("right"))rHand=c; if(n.Contains("sword"))sword=c; }
    var sb=new StringBuilder();
    sb.Append("cam.pos="+cam.position.ToString("0.000")+"\n");
    sb.Append("cam.fwd="+cam.forward.ToString("0.00")+" right="+cam.right.ToString("0.00")+" up="+cam.up.ToString("0.00")+"\n");
    var rc=Center(rHand); sb.Append("RIGHT fist center(world)="+rc.ToString("0.000")+" | camLocal="+cam.InverseTransformPoint(rc).ToString("0.000")+"\n");
    var lc=Center(lh); sb.Append("LEFT fist center(world)="+lc.ToString("0.000")+" | camLocal="+cam.InverseTransformPoint(lc).ToString("0.000")+"\n");
    var handBone=F(rHand,"Hand"); if(handBone!=null) sb.Append("Hand bone world="+handBone.position.ToString("0.000")+"\n");
    if(sword!=null){ var r=sword.GetComponentInChildren<Renderer>(); sb.Append("SWORD center(world)="+r.bounds.center.ToString("0.000")+" size="+r.bounds.size.ToString("0.00")+" | camLocal="+cam.InverseTransformPoint(r.bounds.center).ToString("0.000")+"\n"); }
    return sb.ToString();
  }
}
