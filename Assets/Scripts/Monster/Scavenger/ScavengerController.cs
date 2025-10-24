using UnityEngine;
using System.Collections;

public class ScavengerController : BaseMonster
{
    public float explosionRange = 35f;
    public float explosionDelay = 1.5f;
    public int explosionDamage = 2;

    private float lastAttackTime;
    private bool isExploding = false;

    protected override void Start()
    {
        base.Start();
        attackRange = 15f;   // BaseMonster의 attackRange 사용
        EnemyattackDelay = 2f;    // BaseMonster의 attackDelay 사용
        explosionRange = 35f;
        explosionDelay = 1.5f;
        explosionDamage = 2;
    }

    protected override void Attack()
    {
        // 📌 브루트 공격 로직
        Debug.Log($"{name}이(가) 돌진 공격!");
        // 예시: 플레이어에게 데미지 적용 로직 추가
    }

    void Update()
    {
        if (isDead || isExploding) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time - lastAttackTime > EnemyattackDelay)
        {
            StartCoroutine(NormalAttack());
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    IEnumerator NormalAttack()
    {
        anim.SetTrigger("Attack");
        Debug.Log("Scavenger 공격!");
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(0.5f);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (currentHP <= 0 && !isExploding)
        {
            StartCoroutine(SelfDestruct());
        }
    }

    IEnumerator SelfDestruct()
    {
        isExploding = true;
        anim.SetTrigger("Explode");
        Debug.Log("Scavenger 자폭 준비...");
        GetComponent<SpriteRenderer>().color = Color.magenta;

        yield return new WaitForSeconds(explosionDelay);
        Debug.Log("💥 Scavenger 폭발!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("플레이어 폭발 데미지 입음!");
            }
        }

        Destroy(gameObject);
    }
}
