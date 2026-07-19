using System.Text;
using UnityEngine;

public class CheckEnv
{
    public static string Main()
    {
        var sb = new StringBuilder();
        var env = GameObject.Find("Environment");
        if (env == null) return "no Environment";
        foreach (Transform c in env.transform)
        {
            sb.Append(c.name + " (children=" + c.childCount + ")\n");
            if (c.name == "Trees")
                foreach (Transform t in c)
                    sb.Append("   - " + t.name + " kids=" + t.childCount + "\n");
        }
        return sb.ToString();
    }
}
