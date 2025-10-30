using UnityEngine;

public class Scavenger : BaseMonster
{
    public GameObject explosionEffect;

    protected override void Die()
    {
        isDead = true;
        Debug.Log("��ĳ������ �����մϴ�!");
        if (explosionEffect) Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    protected override void Attack()
    {
        Debug.Log("��ĳ������ ���� ����!");
    }
}
