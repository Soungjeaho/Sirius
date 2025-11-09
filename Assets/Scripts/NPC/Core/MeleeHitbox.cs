using UnityEngine;

namespace Project.NPC
{
    public class MeleeHitbox : MonoBehaviour
    {
        public Project.Combat.DamageDealer2D dealer;

        public int Swing()
        {
            if (!dealer) dealer = GetComponent<Project.Combat.DamageDealer2D>();
            return dealer != null ? dealer.DealOnce() : 0;
        }
    }
}
