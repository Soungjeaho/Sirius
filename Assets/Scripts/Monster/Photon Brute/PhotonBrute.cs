using UnityEngine;

public class PhotonBrute : BaseMonster
{
    public float chargeSpeed = 6f;
    public float smashRange = 2f;
    public float skillCooldown = 3f;
    private float lastSkillTime;

    protected override void Attack()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (Time.time - lastSkillTime > skillCooldown && distance > attackRange)
        {
            Debug.Log("���� ���Ʈ ���� ����!");
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * chargeSpeed * Time.deltaTime);

            // ������� ȿ��
            if (distance < smashRange)
            {
                Debug.Log("���� ���Ʈ �������!");
            }
            lastSkillTime = Time.time;
        }
        else
        {
            Debug.Log("���� ���Ʈ �Ϲ� ����!");
        }
    }
}
