using UnityEngine;

namespace Project.NPC
{
    public class RangedAttackVision : MonoBehaviour
    {
        public float meleeRange = 1f;
        public float rangedRange = 5f;
        public Transform player;
        public bool inMelee;
        public bool inRanged;

        void Update()
        {
            if (!player)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
            if (!player) { inMelee = inRanged = false; return; }
            float d = Vector2.Distance(transform.position, player.position);
            inMelee = d <= meleeRange;
            inRanged = d <= rangedRange && d > meleeRange;
        }
    }
}
