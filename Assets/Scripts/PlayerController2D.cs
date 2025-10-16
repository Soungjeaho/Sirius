using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move Settings")]
    public float walkSpeed = 3f;      // 1/s �⺻ �̵��ӵ�
    public float runSpeed = 6f;       // �޸��� �ӵ�
    private float currentSpeed;

    [Header("Jump Settings")]
    public float jumpForce = 7f;      // ���� ���� ����
    public float doubleJumpForce = 6f;
    private bool isGrounded = false;
    private bool canDoubleJump = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    SpriteRenderer sr;
    float horiz;
    private float lastRunKeyTime = 0f;
    private KeyCode lastRunKey = KeyCode.None;
    private float runThreshold = 0.5f; // 0.5s ���� ������ �޸���

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentSpeed = walkSpeed;
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = go.transform;
        }
    }

    void Update()
    {
        ReadInput();
        HandleInventoryMapMenu();
    }

    void FixedUpdate()
    {
        CheckGround();
        Move();
    }

    void ReadInput()
    {
        // ��/�� �̵�
        horiz = 0f;
        if (Input.GetKey(KeyCode.A)) horiz = -1f;
        if (Input.GetKey(KeyCode.D)) horiz = 1f;

        // �޸��� (AA / DD)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            // ó�� ���� Ű ���
            lastRunKey = Input.GetKeyDown(KeyCode.A) ? KeyCode.A : KeyCode.D;
            lastRunKeyTime = Time.time;
        }
        if ((Input.GetKey(KeyCode.A) && lastRunKey == KeyCode.A) || (Input.GetKey(KeyCode.D) && lastRunKey == KeyCode.D))
        {
            if (Time.time - lastRunKeyTime <= runThreshold && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                currentSpeed = runSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // ���� / ��������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
                canDoubleJump = false;
            }
        }

        // �Ʒ����� (S)
        if (Input.GetKey(KeyCode.S))
        {
            // ȭ�� ������ ī�޶� �並 �������� ó��(���� ������ ī�޶� y offset ����)
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, -0.5f, Camera.main.transform.localPosition.z);
        }
        else
        {
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, 0f, Camera.main.transform.localPosition.z);
        }

        // ��ȣ�ۿ�(W)
        if (Input.GetKeyDown(KeyCode.W))
        {
            TryInteract();
        }
    }

    void Move()
    {
        Vector2 vel = rb.velocity;
        vel.x = horiz * currentSpeed;
        rb.velocity = vel;

        // �¿� ����(��������Ʈ)
        if (horiz < 0) sr.flipX = true;
        if (horiz > 0) sr.flipX = false;
    }

    void CheckGround()
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = col != null;
        if (isGrounded) canDoubleJump = false; // ���� ������ �ʱ�ȭ
    }

    void TryInteract()
    {
        // ĳ���� �տ� ���� ������ ��ȣ�ۿ� �˻�
        Vector2 dir = sr.flipX ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f, LayerMask.GetMask("Interactable"));
        if (hit.collider != null)
        {
            var interact = hit.collider.GetComponent<Interactable>();
            if (interact != null) interact.OnInteract();
        }
    }

    void HandleInventoryMapMenu()
    {
        // �κ��丮(F) - ����ڼ��迡 ����, �� ���迡�� I
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.I))
        {
            UIManager.Instance.ToggleInventory();
        }



        // ����(TAB)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.Instance.ToggleMap();
        }

        // �޴�(ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.ToggleMenu();
        }
    }

    // �׸���� (�����)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
