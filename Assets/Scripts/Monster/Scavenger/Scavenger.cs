using UnityEngine;

public class Scavenger : BaseMonster
{
    public GameObject explosionEffect;

    protected override void Die()
    {
        isDead = true;
        Debug.Log("스캐빈저가 자폭합니다!");
        if (explosionEffect) Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    protected override void Attack()
    {
        Debug.Log("스캐빈저가 근접 공격!");
    }
}
