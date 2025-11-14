using UnityEngine;

namespace Project.Combat
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int maxHP = 5;
        public int currentHP;
        public bool invincible;
        public float hurtCooldown = 0.3f;
        private float _lastHurt;

        public bool IsDead => currentHP <= 0;

        void Awake()
        {
            currentHP = maxHP;
        }

        public void ApplyDamage(int amount, Vector2 hitPoint, Vector2 hitNormal, Object source = null)
        {
            if (invincible || IsDead) return;
            if (Time.time - _lastHurt < hurtCooldown) return;

            currentHP -= Mathf.Max(1, amount);
            _lastHurt = Time.time;

            // TODO: VFX/SFX/knockback
            if (currentHP <= 0)
            {
                // TODO: death/respawn
            }
        }
    }
}
