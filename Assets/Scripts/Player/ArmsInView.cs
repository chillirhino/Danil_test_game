using UnityEngine;

namespace PoK.Player
{
    /// <summary>
    /// Keeps the first-person arms extended forward into the camera view each frame
    /// (on top of the frozen idle pose), so the hands are actually visible while the
    /// torso stays behind the camera. The right arm is left alone during the attack
    /// slash so the swing animation plays.
    /// </summary>
    public class ArmsInView : MonoBehaviour
    {
        public Transform cameraTransform;
        [Header("Where the hands sit in view (camera-relative)")]
        public float forward = 0.42f;
        public float side = 0.15f;
        public float down = 0.22f;

        Animator _anim;
        Transform _rUp, _rHand, _lUp, _lHand;

        void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            // Aim the FOREARM (bend the elbow) so the hand rises into view from the
            // bottom while the shoulder stays low/out of frame — proper FP arm framing.
            _rUp = Find("PT_RightForeArm");
            _rHand = Find("PT_RightHand");
            _lUp = Find("PT_LeftForeArm");
            _lHand = Find("PT_LeftHand");
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;

            Vector3 rT = cameraTransform.position + cameraTransform.forward * forward
                       + cameraTransform.right * side - cameraTransform.up * down;
            Vector3 lT = cameraTransform.position + cameraTransform.forward * forward
                       - cameraTransform.right * side - cameraTransform.up * down;

            bool slashing = _anim != null && _anim.layerCount > 1 &&
                            _anim.GetCurrentAnimatorStateInfo(1).IsName("Slash");

            if (!slashing) Aim(_rUp, _rHand, rT);
            Aim(_lUp, _lHand, lT);
        }

        static void Aim(Transform upper, Transform hand, Vector3 target)
        {
            if (upper == null || hand == null) return;
            Vector3 cur = hand.position - upper.position;
            Vector3 tgt = target - upper.position;
            upper.rotation = Quaternion.FromToRotation(cur, tgt) * upper.rotation;
        }

        Transform Find(string n) { return FindRec(transform, n); }
        static Transform FindRec(Transform r, string n)
        {
            if (r.name == n) return r;
            foreach (Transform c in r) { var x = FindRec(c, n); if (x != null) return x; }
            return null;
        }
    }
}
