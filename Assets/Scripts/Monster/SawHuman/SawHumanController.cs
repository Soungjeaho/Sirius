using UnityEngine;
using System.Collections;

public class SawHumanController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public float detectRange = 100f;
    public float attackRange = 25f;
    public float dashRange = 100f;

    public float jumpDuration = 0.5f;
    public float dashSpeed = 10f;

    [Header("Animator & Rigidbody")]
    public Animator anim;
    public Rigidbody2D rb;

    private Transform player;
    private bool isDashing = false;
    private float lastAttackTime = 0f;
    private float attackDelay = 2f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (isDashing) return;

        if (distance <= dashRange && Time.time - lastAttackTime > attackDelay)
        {
            StartCoroutine(JumpAttack());
        }
        else if (distance <= attackRange && Time.time - lastAttackTime > attackDelay)
        {
            StartCoroutine(Slash2Hit());
        }
        else if (distance > attackRange)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 dir = new Vector2(player.position.x - transform.position.x, 0).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
        anim.SetBool("isMoving", true);
    }

    IEnumerator JumpAttack()
    {
        isDashing = true;
        anim.SetTrigger("AttackJump");
        lastAttackTime = Time.time;

        Vector2 dir = (player.position - transform.position).normalized;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            rb.MovePosition(rb.position + dir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1f); // 경직
        isDashing = false;
    }

    IEnumerator Slash2Hit()
    {
        anim.SetTrigger("Slash2Hit");
        lastAttackTime = Time.time;

        for (int i = 0; i < 2; i++)
        {
            Debug.Log("횡베기 공격!");
            yield return new WaitForSeconds(0.5f);
        }
    }
}
