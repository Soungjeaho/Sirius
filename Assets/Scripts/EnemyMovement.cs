using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public bool moveRight = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Reelback reelback = PlayerController_Sungmin.Instance.GetComponent<Reelback>();

        if (IsBeingGrappled(reelback))
            PullToPlayer(reelback);
        else
            Patrol();
    }

    private bool IsBeingGrappled(Reelback reelback)
    {
        if (reelback == null) return false;
        return reelback.IsPullingEnemy && reelback.GetPulledEnemy() == rb;
    }

    private void Patrol()
    {
        Vector2 velocity = rb.velocity;
        velocity.x = moveRight ? moveSpeed : -moveSpeed;
        rb.velocity = velocity;
    }

    private void PullToPlayer(Reelback reelback)
    {
        Vector2 playerPos = PlayerController_Sungmin.Instance.transform.position;
        float grappleSpeed = reelback.ReelbackSpeed;

        Vector2 newPos = Vector2.MoveTowards(rb.position, playerPos, grappleSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        rb.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Reelbackable"))
            moveRight = !moveRight;
    }
}
