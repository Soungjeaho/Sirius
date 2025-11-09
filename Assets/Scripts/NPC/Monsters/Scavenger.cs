using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class Scavenger : EnemyBase
    {
        public float explodeRange = 1.2f;
        public int triggerHP = 1;
        public float armTime = 0.6f;
        public float explodeRadius = 1.8f;
        private bool armed;

        protected override void TryMelee()
        {
            if (_busy) return;
            StartCoroutine(Bite());
        }

        protected override void TrySkill(float d)
        {
            if (armed && d <= explodeRange) StartCoroutine(Explode());
        }

        public override void TakeDamage(int dmg = 1)
        {
            base.TakeDamage(dmg);
            if (!IsDead && !armed && _hp <= triggerHP)
            {
                StartCoroutine(Arm());
            }
        }

        IEnumerator Arm()
        {
            armed = true;
            _state = EnemyState.Skill;
            animParams.Fire(anim, animParams.skillATrig);
            yield return new WaitForSeconds(armTime);
            ChangeState(EnemyState.Chase);
        }

        IEnumerator Bite()
        {
            _lastAttack = Time.time; _state = EnemyState.Attack; _busy = true;
            animParams.Fire(anim, animParams.attackTrig);
            yield return new WaitForSeconds(0.1f);
            DealAoE(attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position, 0.7f, 1);
            yield return new WaitForSeconds(0.3f);
            _busy = false; ChangeState(EnemyState.Chase);
        }

        IEnumerator Explode()
        {
            _state = EnemyState.Skill; _busy = true;
            animParams.Fire(anim, animParams.skillBTrig);
            yield return new WaitForSeconds(0.2f);
            DealAoE(transform.position, explodeRadius, 3);
            Die();
        }
    }
}
