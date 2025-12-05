using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class SwordHuman : EnemyBase
    {
        public float shockRange = 5f;
        public float airTime = 0.35f;
        public float shockRadius = 1.2f;
        public MeleeHitbox heavy;

        protected override void TryMelee()
        {
            if (_busy) return;
            StartCoroutine(HeavyTwo());
        }

        protected override void TrySkill(float d)
        {
            if (_busy) return;
            if (d > meleeRange && d >= shockRange && Time.time >= _lastAttack + attackDelay)
                StartCoroutine(JumpSlam());
        }

        IEnumerator JumpSlam()
        {
            _lastAttack = Time.time; _state = EnemyState.Skill; _busy = true;
            animParams.Fire(anim, animParams.skillATrig);
            Vector2 s = transform.position;
            float dir = _facingRight ? 1 : -1; Vector2 e = s + new Vector2(dir * 1.5f, 0);
            float t = 0; while (t < airTime)
            {
                t += Time.deltaTime;
                float h = Mathf.Sin((t / airTime) * Mathf.PI) * 1.0f;
                Vector2 pos = Vector2.Lerp(s, e, t / airTime) + new Vector2(0, h);
                rb.MovePosition(pos);
                yield return null;
            }
            DealAoE((Vector2)transform.position + new Vector2(dir * 0.5f, 0), shockRadius, 2);
            yield return new WaitForSeconds(0.5f);
            _busy = false; ChangeState(EnemyState.Chase);
        }

        IEnumerator HeavyTwo()
        {
            _lastAttack = Time.time; _state = EnemyState.Attack; _busy = true;
            animParams.Fire(anim, animParams.attackTrig);
            yield return new WaitForSeconds(0.15f);
            if (heavy) heavy.Swing();
            yield return new WaitForSeconds(0.25f);
            if (heavy) heavy.Swing();
            yield return new WaitForSeconds(0.45f);
            _busy = false; ChangeState(EnemyState.Chase);
        }
    }
}
