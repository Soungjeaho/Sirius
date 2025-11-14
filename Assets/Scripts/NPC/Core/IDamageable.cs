using UnityEngine;

namespace Project.Combat
{
    public interface IDamageable
    {
        void ApplyDamage(int amount, Vector2 hitPoint, Vector2 hitNormal, Object source = null);
        bool IsDead { get; }
    }
}
