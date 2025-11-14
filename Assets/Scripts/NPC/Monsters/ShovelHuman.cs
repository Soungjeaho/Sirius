using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class ShovelHuman : EnemyBase
    {
        public float throwRange = 4.5f;
        public float dirtRadius = 1.0f;
        public float windup = 0.3f;
        public MeleeHitbox hitbox;

        protected override void TryMelee()
        {
            if (_busy) return;
            StartCoroutine(Swing());
        }

        protected override void TrySkill(float d)
        {
            if (_busy) return;
            if (d > meleeRange && d <= throwRange && Time.time >= _lastAttack + attackDelay)
                StartCoroutine(DirtThrow());
        }

        IEnumerator Swing()
        {
            _lastAttack = Time.time; _state = EnemyState.Attack; _busy = true;
            animParams.Fire(anim, animParams.attackTrig);
            yield return new WaitForSeconds(0.12f);
            if (hitbox) hitbox.Swing();
            yield return new WaitForSeconds(0.4f);
            _busy = false; ChangeState(EnemyState.Chase);
        }

        IEnumerator DirtThrow()
        {
            _lastAttack = Time.time; _state = EnemyState.Skill; _busy = true;
            animParams.Fire(anim, animParams.skillATrig);
            yield return new WaitForSeconds(windup);
            Vector2 pos = attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position;
            DealAoE(pos + new Vector2(_facingRight ? 1f : -1f, 0), dirtRadius, 1);
            yield return new WaitForSeconds(0.5f);
            _busy = false; ChangeState(EnemyState.Chase);
        }
    }
}
