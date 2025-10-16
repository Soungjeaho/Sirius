using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move Settings")]
    public float walkSpeed = 3f;      // 1/s 기본 이동속도
    public float runSpeed = 6f;       // 달리기 속도
    private float currentSpeed;

    [Header("Jump Settings")]
    public float jumpForce = 7f;      // 점프 높이 조절
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
    private float runThreshold = 0.5f; // 0.5s 내에 누르면 달리기

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
        // 좌/우 이동
        horiz = 0f;
        if (Input.GetKey(KeyCode.A)) horiz = -1f;
        if (Input.GetKey(KeyCode.D)) horiz = 1f;

        // 달리기 (AA / DD)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            // 처음 누른 키 기억
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

        // 점프 / 더블점프
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

        // 아래보기 (S)
        if (Input.GetKey(KeyCode.S))
        {
            // 화면 내에서 카메라나 뷰를 내려보는 처리(간단 구현은 카메라 y offset 조절)
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, -0.5f, Camera.main.transform.localPosition.z);
        }
        else
        {
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, 0f, Camera.main.transform.localPosition.z);
        }

        // 상호작용(W)
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

        // 좌우 반전(스프라이트)
        if (horiz < 0) sr.flipX = true;
        if (horiz > 0) sr.flipX = false;
    }

    void CheckGround()
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = col != null;
        if (isGrounded) canDoubleJump = false; // 지면 닿으면 초기화
    }

    void TryInteract()
    {
        // 캐릭터 앞에 작은 범위로 상호작용 검사
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
        // 인벤토리(F) - 사용자설계에 따라, 너 설계에선 I
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.I))
        {
            UIManager.Instance.ToggleInventory();
        }



        // 지도(TAB)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.Instance.ToggleMap();
        }

        // 메뉴(ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.ToggleMenu();
        }
    }

    // 그리기용 (디버그)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
