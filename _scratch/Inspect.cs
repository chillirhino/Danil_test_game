using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class Inspect
{
    public static string Main()
    {
        var sb = new StringBuilder();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        sb.Append("roots: ");
        foreach (var go in scene.GetRootGameObjects()) sb.Append(go.name + ", ");
        sb.Append("\n");

        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
            sb.Append("Light " + l.name + " type=" + l.type + " intensity=" + l.intensity + " rot=" + l.transform.eulerAngles + "\n");

        var vols = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        sb.Append("Volumes=" + vols.Length + "\n");
        foreach (var v in vols)
            sb.Append("Volume " + v.name + " global=" + v.isGlobal + " profile=" + (v.sharedProfile != null ? v.sharedProfile.name : "null") + "\n");

        sb.Append("ambientMode=" + RenderSettings.ambientMode + " skybox=" + (RenderSettings.skybox != null ? RenderSettings.skybox.name : "null") + "\n");
        return sb.ToString();
    }
}
