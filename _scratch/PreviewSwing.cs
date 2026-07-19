using UnityEngine;
public class PreviewSwing {
  public static string Main(){
    var hand=GameObject.Find("WeaponHand");
    if(hand==null) return "no hand";
    // apply swing peak: base + offset (base pos (0.22,-0.35,0.55), rot Z15)
    var basePos=new Vector3(0.22f,-0.35f,0.55f);
    var baseRot=Quaternion.Euler(0f,0f,15f);
    hand.transform.localPosition=basePos+new Vector3(-0.18f,-0.06f,0.12f);
    hand.transform.localRotation=baseRot*Quaternion.Euler(35f,-70f,-45f);
    return "swing peak applied";
  }
}
