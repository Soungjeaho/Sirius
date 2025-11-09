using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class AxeHuman : EnemyBase
    {
        [Header("Axe Spec")]
        public float lungeDistance = 1.2f;
        public float lungeTime = 0.25f;
        public MeleeHitbox hitbox;

        protected override void TryMelee()
        {
            if (_busy) return;
            StartCoroutine(Lunge());
        }

        protected override void TrySkill(float d) { /* 없음 */ }

        IEnumerator Lunge()
        {
            _lastAttack = Time.time;
            _state = EnemyState.Attack;
            _busy = true;
            animParams.Fire(anim, animParams.attackTrig);

            Vector2 s = transform.position;
            Vector2 e = s + new Vector2(_facingRight ? lungeDistance : -lungeDistance, 0);
            float t = 0f;
            while (t < lungeTime)
            {
                t += Time.deltaTime;
                rb.MovePosition(Vector2.Lerp(s, e, t / lungeTime));
                yield return null;
            }

            if (hitbox) hitbox.Swing(); // 근접 판정
            yield return new WaitForSeconds(0.35f); // 후딜

            _busy = false;
            ChangeState(EnemyState.Chase);
        }
    }
}
