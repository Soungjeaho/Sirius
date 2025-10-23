using UnityEngine;
using System.Collections;

public class PhotonBruteController : BaseMonster
{
    public float dashRange = 100f;
    public float smashRange = 35f;
    public float dashSpeed = 12f;
    public float dashDelay = 2f;
    private float lastDashTime;
    private bool isDashing = false;



    protected override void Attack()
    {
        // 📌 근접 공격 로직
        Debug.Log($"{name}이(가) 근접 공격!");
        // 예시: 플레이어 피격 판정 호출
        void Update()
        {
            if (isDead) return;

            float distance = Vector2.Distance(transform.position, player.position);

            if (!isDashing && distance > smashRange && Time.time - lastDashTime > dashDelay)
            {
                StartCoroutine(DashAttack());
            }
            else if (distance <= smashRange)
            {
                StartCoroutine(SmashAttack());
            }
        }

        IEnumerator DashAttack()
        {
            isDashing = true;
            anim.SetTrigger("Dash");
            Debug.Log("PhotonBrute 돌진!");
            Vector2 dir = (player.position - transform.position).normalized;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                rb.MovePosition(rb.position + dir * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);
            isDashing = false;
            lastDashTime = Time.time;
        }

        IEnumerator SmashAttack()
        {
            anim.SetTrigger("Smash");
            Debug.Log("PhotonBrute 내려찍기!");
            yield return new WaitForSeconds(0.5f);
        }
    }
}
