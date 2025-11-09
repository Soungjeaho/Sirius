using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class SawHuman : EnemyBase
    {
        public float dashDistance = 4f;
        public float dashTime = 0.2f;
        public MeleeHitbox stab;
        public MeleeHitbox sweep;
        private bool nextStab = true;
        public float skillRange = 6.5f;

        protected override void TryMelee()
        {
            if (_busy) return;
            StartCoroutine(nextStab ? Stab() : Sweep());
            nextStab = !nextStab;
        }

        protected override void TrySkill(float d)
        {
            if (_busy) return;
            if (d > meleeRange && d <= skillRange && Time.time >= _lastAttack + attackDelay)
                StartCoroutine(DashClose());
        }

        IEnumerator DashClose()
        {
            _lastAttack = Time.time; _state = EnemyState.Skill; _busy = true;
            animParams.Fire(anim, animParams.skillATrig);
            Vector2 s = transform.position;
            Vector2 e = s + new Vector2((_facingRight ? 1 : -1) * dashDistance, 0);
            float t = 0; while (t < dashTime) { t += Time.deltaTime; rb.MovePosition(Vector2.Lerp(s, e, t / dashTime)); yield return null; }
            yield return new WaitForSeconds(0.12f);
            _busy = false; ChangeState(EnemyState.Chase);
        }

        IEnumerator Stab()
        {
            _lastAttack = Time.time; _state = EnemyState.Attack; _busy = true;
            animParams.Fire(anim, animParams.attackTrig);
            yield return new WaitForSeconds(0.1f);
            if (stab) stab.Swing();
            yield return new WaitForSeconds(0.35f);
            _busy = false; ChangeState(EnemyState.Chase);
        }

        IEnumerator Sweep()
        {
            _lastAttack = Time.time; _state = EnemyState.Attack; _busy = true;
            animParams.Fire(anim, animParams.skillBTrig);
            yield return new WaitForSeconds(0.15f);
            if (sweep) sweep.Swing();
            yield return new WaitForSeconds(0.45f);
            _busy = false; ChangeState(EnemyState.Chase);
        }
    }
}
