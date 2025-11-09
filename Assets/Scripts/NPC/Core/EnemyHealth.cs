using UnityEngine;

namespace Project.Combat
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public Project.NPC.EnemyBase owner;
        public bool IsDead => owner && owner.IsDead;

        void Reset()
        {
            owner = GetComponent<Project.NPC.EnemyBase>();
        }

        public void ApplyDamage(int amount, Vector2 hitPoint, Vector2 hitNormal, Object source = null)
        {
            if (owner) owner.TakeDamage(amount);
        }
    }
}
