using UnityEngine;
using UnityEditor;
public class Stylize {
  static void Flat(string name, Color c){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/"+name+".mat");
    if(m==null) return;
    if(m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", null);
    if(m.HasProperty("_BumpMap")){ m.SetTexture("_BumpMap", null); m.DisableKeyword("_NORMALMAP"); }
    m.SetColor("_BaseColor", c);
    if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0f);
    EditorUtility.SetDirty(m);
  }
  public static string Main(){
    // stylized palette matching the low-poly trees
    Flat("Grass", new Color(0.36f,0.52f,0.24f));
    Flat("Dirt",  new Color(0.50f,0.39f,0.26f));
    AssetDatabase.SaveAssets();
    return "stylized ground+path";
  }
}
