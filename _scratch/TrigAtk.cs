using UnityEngine;
public class TrigAtk { public static string Main(){ var b=GameObject.Find("FPBody"); var a=b.GetComponentInChildren<Animator>(); a.SetTrigger("Attack"); return "triggered"; } }
