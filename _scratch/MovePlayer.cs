using UnityEngine;

public class MovePlayer
{
    public static string Main()
    {
        var p = GameObject.Find("Player");
        if (p == null) return "no player";
        // Stand just in front of the tree at (-6,0,9), facing it.
        p.transform.position = new Vector3(-6f, 1f, 5.5f);
        p.transform.rotation = Quaternion.identity;
        var fpc = p.GetComponent<PoK.Player.FirstPersonController>();
        if (fpc != null) fpc.autoWalk = false; // hold still for the close-up
        return "moved player near tree";
    }
}
