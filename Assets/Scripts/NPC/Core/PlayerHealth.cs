using UnityEngine;

namespace Project.Combat
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Player HP")]
        public int maxHP = 10;
        public int currentHP;

        [Header("Death")]
        public bool destroyOnDeath = false;

        public bool IsDead => currentHP <= 0;

        void Awake()
        {
            currentHP = maxHP;
        }

        public void ApplyDamage(int amount, Vector2 hitPoint, Vector2 hitNormal, Object source = null)
        {
            if (IsDead) return;

            int before = currentHP;
            currentHP = Mathf.Max(0, currentHP - amount);

            Debug.Log($"[PlayerHealth] {name} 피격! {amount} 데미지, HP {before} → {currentHP} (from: {source})");

            if (IsDead)
            {
                Debug.Log($"[PlayerHealth] {name} 사망!");

                // 간단히 움직임만 막기
                var ctrl = GetComponent<Project.Player.SimplePlayerController>();
                if (ctrl) ctrl.enabled = false;

                if (destroyOnDeath)
                    Destroy(gameObject, 1.5f);
            }
        }
    }
}
