using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ArmKeyer
{
    static bool IsBg(Color32 c)
    {
        int mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        int mn = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
        int spread = mx - mn;      // neutral grey => small spread
        return spread < 30 && mx > 70; // light-ish neutral => background / shadow
    }

    static string Process(string assetPath)
    {
        string full = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        byte[] bytes = File.ReadAllBytes(full);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(bytes)) return assetPath + " decode failed";

        int w = tex.width, h = tex.height;
        var px = tex.GetPixels32();
        int n = w * h;
        var bg = new bool[n];
        var q = new Queue<int>();

        // Seed flood fill from every border pixel that looks like background.
        for (int x = 0; x < w; x++)
        {
            TrySeed(px, bg, q, x + 0 * w);
            TrySeed(px, bg, q, x + (h - 1) * w);
        }
        for (int y = 0; y < h; y++)
        {
            TrySeed(px, bg, q, 0 + y * w);
            TrySeed(px, bg, q, (w - 1) + y * w);
        }

        while (q.Count > 0)
        {
            int i = q.Dequeue();
            int x = i % w, y = i / w;
            if (x > 0) TrySeed(px, bg, q, i - 1);
            if (x < w - 1) TrySeed(px, bg, q, i + 1);
            if (y > 0) TrySeed(px, bg, q, i - w);
            if (y < h - 1) TrySeed(px, bg, q, i + w);
        }

        int removed = 0;
        for (int i = 0; i < n; i++)
        {
            if (bg[i]) { px[i].a = 0; removed++; }
            else px[i].a = 255;
        }

        tex.SetPixels32(px);
        tex.Apply();
        File.WriteAllBytes(full, tex.EncodeToPNG());

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }
        float pct = 100f * removed / n;
        return assetPath + " removed=" + pct.ToString("0.0") + "%";
    }

    static void TrySeed(Color32[] px, bool[] bg, Queue<int> q, int i)
    {
        if (bg[i]) return;
        if (!IsBg(px[i])) return;
        bg[i] = true;
        q.Enqueue(i);
    }

    public static string Main()
    {
        string a = Process("Assets/Art/Arms/LeftArm.png");
        string b = Process("Assets/Art/Arms/RightArm.png");
        return a + " | " + b;
    }
}
