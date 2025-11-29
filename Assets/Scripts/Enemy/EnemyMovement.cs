using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool moveRight = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Patrol();
    }

    private void Patrol()
    {
        Vector2 velocity = rb.velocity;
        velocity.x = moveRight ? moveSpeed : -moveSpeed;
        rb.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("RB_Wall") 
         || collision.collider.CompareTag("Ground") 
         || collision.collider.CompareTag("Wall") 
         || collision.collider.CompareTag("Reelbackable") 
         || collision.collider.CompareTag("Obstacle"))
        {
            Flip();
        }
    }


    private void Flip()
    {
        moveRight = !moveRight;

        // 현재 회전값을 가져와서 Y축 기준으로 180도 회전
        Quaternion rotation = transform.rotation;
        rotation = Quaternion.Euler(0f, moveRight ? 0f : 180f, 0f);
        transform.rotation = rotation;
    }
}
