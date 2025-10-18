using UnityEngine;

public class AxeHumanController : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float stopDistance = 1f; // Player와 멈출 거리
    public Animator animator;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        if (player == null) return;

        // Player와 x축 거리만 계산
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);

        if (distanceX > stopDistance)
        {
            // 이동
            float dirX = player.position.x > transform.position.x ? 1 : -1;
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
            animator.SetBool("IsMoving", true);

            // 스프라이트 좌우 반전
            if (dirX < 0) GetComponent<SpriteRenderer>().flipX = true;
            else GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            // Player 가까이 도달 → 멈춤
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("IsMoving", false);

            // 공격
            animator.SetTrigger("Attack");
        }
    }
}
