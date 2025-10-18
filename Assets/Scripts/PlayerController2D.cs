using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("�̵� �ӵ�")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    private float currentSpeed;

    [Header("���� ����")]
    public float jumpForce = 7f;
    public float doubleJumpForce = 6f;
    private bool isGrounded = false;
    private bool canDoubleJump = false;

    [Header("�޸��� ����")]
    public float doubleTapTime = 0.5f;
    private float lastKeyTime = -1f;
    private KeyCode lastKey;
    private bool isRunning = false;

    [Header("��ȣ�ۿ� Ű")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI ����")]
    public GameObject inventoryPanel;
    public GameObject mapPanel;
    public GameObject menuPanel;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentSpeed = walkSpeed;

        // UI ��Ȱ��ȭ
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    void HandleInput()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
            HandleRun(KeyCode.A);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
            HandleRun(KeyCode.D);
        }

        // ���� / ��������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump(jumpForce);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                Jump(doubleJumpForce);
                canDoubleJump = false;
            }
        }

        // ��ȣ�ۿ�
        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }

        // �κ��丮
        if (Input.GetKeyDown(KeyCode.I) && inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }

        // ����
        if (Input.GetKeyDown(KeyCode.Tab) && mapPanel != null)
        {
            mapPanel.SetActive(!mapPanel.activeSelf);
        }

        // �޴�
        if (Input.GetKeyDown(KeyCode.Escape) && menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }

    void Move()
    {
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // ��������Ʈ ����
        if (moveInput < 0) sr.flipX = true;
        if (moveInput > 0) sr.flipX = false;
    }

    void Jump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    void HandleRun(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            if (lastKey == key && (Time.time - lastKeyTime) <= doubleTapTime)
            {
                isRunning = true;
            }

            lastKey = key;
            lastKeyTime = Time.time;
        }

        if (Input.GetKey(key))
        {
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }

        if (Input.GetKeyUp(key))
        {
            isRunning = false;
            currentSpeed = walkSpeed;
        }
    }

    void Interact()
    {
        // 2D�� Physics2D ������ (�ֺ� ��ȣ�ۿ� Ž��)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Interactable"))
            {
                var interactable = hit.GetComponent<Interactable>();
                if (interactable != null)
                {
                    interactable.OnInteract();
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // ����׿� - ��ȣ�ۿ� �ݰ� ǥ��
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
