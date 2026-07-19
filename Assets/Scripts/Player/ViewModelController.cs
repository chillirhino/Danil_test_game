using UnityEngine;
using UnityEngine.InputSystem;

namespace PoK.Player
{
    /// <summary>
    /// Drives the first-person weapon/arms viewmodel:
    /// static idle, gentle walk-bob, and a fast left slash swing on attack
    /// (Path of Kings style — static, then a sharp lunge to the left).
    /// </summary>
    public class ViewModelController : MonoBehaviour
    {
        [Header("What swings (the weapon-hand root)")]
        public Transform weaponHand;

        [Header("Movement source")]
        public FirstPersonController controller;

        [Header("Walk bob")]
        public float bobAmount = 0.015f;
        public float bobFrequency = 6f;

        [Header("Attack swing")]
        public float swingDuration = 0.32f;
        [Tooltip("Fraction of the swing spent lunging out before recovering.")]
        [Range(0.1f, 0.6f)] public float swingOutFraction = 0.28f;
        public Vector3 swingPosOffset = new Vector3(-0.18f, -0.06f, 0.12f);
        public Vector3 swingRotOffset = new Vector3(35f, -70f, -45f);
        public float attackCooldown = 0.15f;

        Vector3 _baseLocalPos;
        Quaternion _baseLocalRot;
        float _swingTimer = -1f;
        float _cooldownTimer;
        float _bobPhase;

        void Start()
        {
            if (controller == null) controller = FindAnyObjectByType<FirstPersonController>();
            if (weaponHand != null)
            {
                _baseLocalPos = weaponHand.localPosition;
                _baseLocalRot = weaponHand.localRotation;
            }
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (AttackPressed() && _cooldownTimer <= 0f && _swingTimer < 0f)
                Attack();

            UpdateSwing();
        }

        public void Attack()
        {
            _swingTimer = 0f;
            _cooldownTimer = swingDuration + attackCooldown;
        }

        void UpdateSwing()
        {
            if (weaponHand == null) return;

            float bob = Mathf.Sin(_bobPhase) * bobAmount;
            float speed = controller != null ? controller.CurrentPlanarSpeed : 0f;
            float maxSpeed = controller != null ? Mathf.Max(0.01f, controller.walkSpeed) : 1f;
            float w = Mathf.Clamp01(speed / maxSpeed);
            _bobPhase += Time.deltaTime * bobFrequency * Mathf.Max(w, 0.15f) * Mathf.PI * 2f;

            Vector3 pos = _baseLocalPos + new Vector3(0f, bob * (0.4f + w), 0f);
            Quaternion rot = _baseLocalRot;

            if (_swingTimer >= 0f)
            {
                _swingTimer += Time.deltaTime;
                float p = _swingTimer / swingDuration;
                if (p >= 1f) { _swingTimer = -1f; p = 1f; }

                float s;
                if (p < swingOutFraction) s = Mathf.SmoothStep(0f, 1f, p / swingOutFraction);
                else s = Mathf.SmoothStep(1f, 0f, (p - swingOutFraction) / (1f - swingOutFraction));

                pos += swingPosOffset * s;
                rot = _baseLocalRot * Quaternion.Euler(swingRotOffset * s);
            }

            weaponHand.localPosition = pos;
            weaponHand.localRotation = rot;
        }

        bool AttackPressed()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;
            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame) return true;
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }
    }
}
