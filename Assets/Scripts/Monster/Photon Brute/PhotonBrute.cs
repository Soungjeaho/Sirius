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
            Debug.Log("포톤 브루트 돌진 공격!");
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * chargeSpeed * Time.deltaTime);

            // 내려찍기 효과
            if (distance < smashRange)
            {
                Debug.Log("포톤 브루트 내려찍기!");
            }
            lastSkillTime = Time.time;
        }
        else
        {
            Debug.Log("포톤 브루트 일반 공격!");
        }
    }
}
