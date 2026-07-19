using UnityEngine; using PoK.Player;
public class T2 { public static string Main(){ var a=GameObject.Find("FPBody").GetComponent<ArmsInView>(); a.forward=0.45f; a.side=0.16f; a.down=0.12f; UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()); return "retuned"; } }
