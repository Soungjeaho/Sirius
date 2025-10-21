using UnityEngine;

public class BaseEnemyController : MonoBehaviour
{
    [Header("Base Stats")]
    public float moveSpeed = 3f;
    public int maxHP = 3;
    protected int currentHP;

    [Header("Detection")]
    public float detectionRange = 10f;
    public Transform player;

    protected Rigidbody2D rb;
    protected Animator anim;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHP = maxHP;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected bool PlayerInRange(float range)
    {
        return Vector2.Distance(transform.position, player.position) <= range;
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0) Die();
    }

    protected virtual void Die()
    {
        anim.SetTrigger("Die");
        Destroy(gameObject, 1f);
    }
}
