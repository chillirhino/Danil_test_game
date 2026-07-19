using UnityEngine;
using UnityEditor;
public class ConvertPoly {
  public static string Main(){
    var urp=Shader.Find("Universal Render Pipeline/Lit");
    int n=0,tx=0;
    foreach(var g in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Polytope Studio"})){
      var p=AssetDatabase.GUIDToAssetPath(g);
      var m=AssetDatabase.LoadAssetAtPath<Material>(p);
      if(m==null||m.shader==null) continue;
      if(!m.shader.name.StartsWith("Polytope")) continue;
      // grab first texture from the custom shader
      Texture tex=null;
      var sh=m.shader;
      int cnt=UnityEditor.ShaderUtil.GetPropertyCount(sh);
      for(int i=0;i<cnt;i++){
        if(UnityEditor.ShaderUtil.GetPropertyType(sh,i)==UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv){
          var pn=UnityEditor.ShaderUtil.GetPropertyName(sh,i);
          var t=m.GetTexture(pn);
          if(t!=null){tex=t;break;}
        }
      }
      m.shader=urp;
      if(tex!=null){m.SetTexture("_BaseMap",tex);tx++;}
      m.SetColor("_BaseColor",Color.white);
      if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",0.1f);
      EditorUtility.SetDirty(m); n++;
    }
    AssetDatabase.SaveAssets();
    return "converted "+n+" Polytope materials ("+tx+" with texture)";
  }
}
