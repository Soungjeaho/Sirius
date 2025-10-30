using UnityEngine;

public class JavelinBomber : BaseMonster
{
    public GameObject bombPrefab;

    protected override void Attack()
    {
        Debug.Log("���� �չ��� ��ź�� ����߸��ϴ�!");
        if (bombPrefab != null)
        {
            Instantiate(bombPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        }
    }
}
