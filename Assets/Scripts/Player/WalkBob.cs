using UnityEngine;

namespace PoK.Player
{
    /// <summary>
    /// Procedural walk-bob for the first-person body: while moving forward the
    /// whole viewmodel (arms + sword) gently bobs and sways, so it feels like
    /// walking — no separate walk animation needed. The attack swing stays separate.
    /// Runs in LateUpdate so it layers on top of the Animator pose.
    /// </summary>
    public class WalkBob : MonoBehaviour
    {
        public FirstPersonController controller;
        public float bobAmount = 0.022f;
        public float swayAmount = 0.016f;
        public float bobFrequency = 5.5f;
        public float rollAmount = 1.4f;
        public float smooth = 8f;

        Vector3 _baseLocalPos;
        Quaternion _baseLocalRot;
        float _phase;
        float _weight;

        void Awake()
        {
            _baseLocalPos = transform.localPosition;
            _baseLocalRot = transform.localRotation;
            if (controller == null) controller = FindAnyObjectByType<FirstPersonController>();
        }

        void LateUpdate()
        {
            float speed = controller != null ? controller.CurrentPlanarSpeed : 0f;
            float maxSpeed = controller != null ? Mathf.Max(0.01f, controller.walkSpeed) : 1f;
            float target = Mathf.Clamp01(speed / maxSpeed);
            _weight = Mathf.Lerp(_weight, target, Time.deltaTime * smooth);

            _phase += Time.deltaTime * bobFrequency * Mathf.Max(_weight, 0.15f) * Mathf.PI * 2f;

            float y = Mathf.Sin(_phase) * bobAmount * _weight;
            float x = Mathf.Cos(_phase * 0.5f) * swayAmount * _weight;
            float roll = Mathf.Cos(_phase * 0.5f) * rollAmount * _weight;

            transform.localPosition = _baseLocalPos + new Vector3(x, y, 0f);
            transform.localRotation = _baseLocalRot * Quaternion.Euler(0f, 0f, roll);
        }
    }
}
