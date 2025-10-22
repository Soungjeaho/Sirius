using UnityEngine;
using System.Collections;

public class BombController : MonoBehaviour
{
    public float explodeDelay = 1f;
    public float damageRadius = 2f;
    public int damage = 2;

    void Start()
    {
        StartCoroutine(ExplodeAfterTime());
    }

    IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(explodeDelay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("�÷��̾� ��ź ������!");
            }
        }

        // ���� �� Prefab ����
        Destroy(gameObject);
    }

    // Inspector�� ���� ǥ��
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }

}
