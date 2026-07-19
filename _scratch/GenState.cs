using UnityEngine;
public class GenState { public static string Main(){
  var p=GameObject.Find("Player"); var g=GameObject.Find("WorldGenerator");
  int chunks=g!=null?g.transform.childCount:-1;
  int props=0; if(g!=null) foreach(Transform c in g.transform) props+=c.GetComponentsInChildren<Transform>().Length;
  return "playerZ="+(p!=null?p.transform.position.z.ToString("0.0"):"?")+" chunks="+chunks+" totalNodes="+props;
}}
