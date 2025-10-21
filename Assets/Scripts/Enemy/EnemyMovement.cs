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
        if (collision.collider.CompareTag("RB_Wall") || collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Wall"))
        {
            Flip();
        }
    }


    private void Flip()
    {
        moveRight = !moveRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}
