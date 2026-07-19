using UnityEngine;
using UnityEditor;
public class SetupAnims {
  static string Humanize(string path, bool loop){
    var imp=AssetImporter.GetAtPath(path) as ModelImporter;
    if(imp==null) return path+" no importer";
    imp.animationType=ModelImporterAnimationType.Human;
    imp.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
    // loop the idle
    var clips=imp.defaultClipAnimations;
    if(clips.Length>0){ clips[0].loopTime=loop; imp.clipAnimations=clips; }
    imp.SaveAndReimport();
    // report resulting clip + avatar
    var av=AssetDatabase.LoadAllAssetsAtPath(path);
    string clipName="?"; bool human=false;
    foreach(var a in av){ if(a is AnimationClip ac && !ac.name.StartsWith("__")) clipName=ac.name; if(a is Avatar avt) human=avt.isHuman; }
    return path.Substring(path.LastIndexOf('/')+1)+" -> clip='"+clipName+"' humanAvatar="+human;
  }
  public static string Main(){
    string a=Humanize("Assets/Sword And Shield Idle.fbx", true);
    string b=Humanize("Assets/Stable Sword Inward Slash.fbx", false);
    return a+" | "+b;
  }
}
