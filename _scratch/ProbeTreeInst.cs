using System.Text;
using UnityEngine;

public class ProbeTreeInst
{
    public static string Main()
    {
        var env = GameObject.Find("Environment");
        var trees = env.transform.Find("Trees");
        var sb = new StringBuilder();
        int n = 0;
        foreach (Transform inst in trees)
        {
            if (n++ >= 2) break;
            sb.Append("INST " + inst.name + " active=" + inst.gameObject.activeInHierarchy
                + " pos=" + inst.position + " scale=" + inst.localScale + "\n");
            var comps = inst.GetComponents<Component>();
            foreach (var c in comps) sb.Append("   comp: " + c.GetType().Name + "\n");
            var mf = inst.GetComponent<MeshFilter>();
            if (mf != null) sb.Append("   mesh=" + (mf.sharedMesh != null ? mf.sharedMesh.name + " verts=" + mf.sharedMesh.vertexCount : "NULL") + "\n");
            var mr = inst.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                sb.Append("   MR enabled=" + mr.enabled + " bounds=" + mr.bounds.size + " mats=");
                foreach (var m in mr.sharedMaterials) sb.Append((m != null ? m.name : "NULL") + ",");
                sb.Append("\n");
            }
            var lod = inst.GetComponent<LODGroup>();
            if (lod != null) sb.Append("   LODGroup lods=" + lod.lodCount + "\n");
            var tree = inst.GetComponent<Tree>();
            if (tree != null) sb.Append("   Tree component present\n");
        }
        return sb.ToString();
    }
}
