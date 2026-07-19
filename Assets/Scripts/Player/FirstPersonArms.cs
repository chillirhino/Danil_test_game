using UnityEngine;

namespace PoK.Player
{
    /// <summary>
    /// Drives the 2D first-person arm sprites (left = torch hand, right = weapon hand).
    /// Bobs them up/down while walking and adds a gentle idle sway — the "2.5D" look
    /// from the Path of Kings reference (enemi.png).
    /// </summary>
    public class FirstPersonArms : MonoBehaviour
    {
        [Header("Arm sprites (RectTransforms on the HUD canvas)")]
        public RectTransform leftArm;
        public RectTransform rightArm;

        [Header("Source of movement")]
        public FirstPersonController controller;

        [Header("Walk bob")]
        [Tooltip("How far the arms travel up/down while walking (pixels).")]
        public float walkBobAmplitude = 28f;
        [Tooltip("Sideways sway while walking (pixels).")]
        public float walkSwayAmplitude = 14f;
        [Tooltip("Steps per second at full walk speed.")]
        public float walkBobFrequency = 5.5f;

        [Header("Idle sway")]
        public float idleAmplitude = 6f;
        public float idleFrequency = 1.2f;

        [Header("Feel")]
        [Tooltip("How quickly the bob amount reacts to starting/stopping.")]
        public float responsiveness = 8f;

        Vector2 _leftBase, _rightBase;
        float _phase;
        float _walkWeight; // 0 = idle, 1 = full walk, smoothed

        void Awake()
        {
            if (leftArm != null) _leftBase = leftArm.anchoredPosition;
            if (rightArm != null) _rightBase = rightArm.anchoredPosition;
            if (controller == null)
                controller = FindAnyObjectByType<FirstPersonController>();
        }

        void Update()
        {
            float speed = controller != null ? controller.CurrentPlanarSpeed : 0f;
            float maxSpeed = controller != null ? Mathf.Max(0.01f, controller.walkSpeed) : 1f;
            float targetWeight = Mathf.Clamp01(speed / maxSpeed);

            _walkWeight = Mathf.Lerp(_walkWeight, targetWeight, Time.deltaTime * responsiveness);

            // Advance the walk cycle only while moving; idle uses real time.
            _phase += Time.deltaTime * walkBobFrequency * Mathf.Max(_walkWeight, 0.0001f) * Mathf.PI * 2f;

            float bob = Mathf.Sin(_phase) * walkBobAmplitude * _walkWeight;
            float sway = Mathf.Cos(_phase * 0.5f) * walkSwayAmplitude * _walkWeight;

            float idle = Mathf.Sin(Time.time * idleFrequency * Mathf.PI * 2f) * idleAmplitude * (1f - _walkWeight);

            // Left and right arms bob in opposite phase for a natural gait.
            if (leftArm != null)
                leftArm.anchoredPosition = _leftBase + new Vector2(sway, bob + idle);
            if (rightArm != null)
                rightArm.anchoredPosition = _rightBase + new Vector2(-sway, -bob + idle);
        }
    }
}
