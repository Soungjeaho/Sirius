using UnityEngine;

public class AxeHumanController : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float stopDistance = 1f; // Player�� ���� �Ÿ�
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

        // Player�� x�� �Ÿ��� ���
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);

        if (distanceX > stopDistance)
        {
            // �̵�
            float dirX = player.position.x > transform.position.x ? 1 : -1;
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
            animator.SetBool("IsMoving", true);

            // ��������Ʈ �¿� ����
            if (dirX < 0) GetComponent<SpriteRenderer>().flipX = true;
            else GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            // Player ������ ���� �� ����
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("IsMoving", false);

            // ����
            animator.SetTrigger("Attack");
        }
    }
}
