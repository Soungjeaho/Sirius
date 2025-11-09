using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class PhotonBrute : EnemyBase
    {
        public float dashRange = 7.5f;
        public float smashRange = 2.2f;
        public float dashSpeed = 12f;
        public float dashCD = 2f;
        private float lastDash;
        public MeleeHitbox smashHit;

        protected override void TryMelee()
        {
            if (_busy) return;
            if (_player && Vector2.Distance(transform.position, _player.position) <= smashRange)
                StartCoroutine(Smash());
        }

        protected override void TrySkill(float d)
        {
            if (_busy) return;
            if (d > smashRange && d <= dashRange && Time.time - lastDash > dashCD)
                StartCoroutine(Dash());
        }

        IEnumerator Dash()
        {
            lastDash = Time.time; _state = EnemyState.Skill; _busy = true;
            animParams.Fire(anim, animParams.skillATrig);
            float t = 0, time = 0.22f;
            Vector2 s = transform.position;
            Vector2 e = s + new Vector2((_facingRight ? 1 : -1) * 3.5f, 0);
            while (t < time) { t += Time.deltaTime; rb.MovePosition(Vector2.Lerp(s, e, t / time)); yield return null; }
            yield return new WaitForSeconds(0.1f);
            _busy = false; ChangeState(EnemyState.Chase);
        }

        IEnumerator Smash()
        {
            _lastAttack = Time.time; _state = EnemyState.Attack; _busy = true;
            animParams.Fire(anim, animParams.attackTrig);
            yield return new WaitForSeconds(0.18f);
            if (smashHit) smashHit.Swing();
            yield return new WaitForSeconds(0.5f);
            _busy = false; ChangeState(EnemyState.Chase);
        }
    }
}
