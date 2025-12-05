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
        public GameObject spinVfxPrefab;

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
            _lastAttack = Time.time;
            _state = EnemyState.Attack;
            _busy = true;

            // 회전베기 애니메이션 트리거
            animParams.Fire(anim, animParams.skillBTrig);

            // 타격 타이밍까지 약간 대기
            yield return new WaitForSeconds(0.2f);

            // 1) 히트박스 발동
            if (sweep)
                sweep.Swing();

            // 2) 회전베기 이펙트 생성
            if (spinVfxPrefab != null)
            {
                // 이펙트 위치는 AttackOrigin 기준이 자연스럽다
                Vector3 spawnPos = attackOrigin ? attackOrigin.position : transform.position;

                var vfx = Instantiate(spinVfxPrefab, spawnPos, Quaternion.identity);

                // 파티클이면 자동 삭제 처리
                var ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(vfx, 2f); // 안전하게 2초 뒤 정리
                }
            }

            // 후딜
            yield return new WaitForSeconds(0.3f);

            _busy = false;
            ChangeState(EnemyState.Chase);
        }

    }
}
