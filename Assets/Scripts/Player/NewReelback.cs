using System.Collections;
using UnityEngine;

public class NewReelback : MonoBehaviour
{
    [SerializeField] GameObject HookPrefab = null;
    [SerializeField] SpriteRenderer playerSprite = null;
    [SerializeField] float maxDistance = 10f;
    [SerializeField] float hookSpeed = 20f;
    [SerializeField] public Transform FirePoint = null;
    [SerializeField] public LineRenderer lr = null;
    [SerializeField] Rigidbody2D rb = null;
    [HideInInspector] public bool isEnemyBeingGrappled = false;

    private Vector2 FireDirection;
    Camera m_cam = null;
    bool facingRight = true;
    GameObject currentHook = null;

    bool hookTriggered = false;
    Vector2 fixedHookPosition;

    [Header("Reelbackable 당기기 관련")]
    private GameObject pullTarget = null;
    private bool isPullingObject = false;
    private float pullSpeed = 5f;

    public bool IsGrappling { get; private set; } = false;

    private void Start()
    {
        m_cam = Camera.main;

        if (FirePoint == null)
            Debug.LogError("FirePoint가 할당되지 않았습니다!");
        if (playerSprite == null)
            Debug.LogError("Player Sprite가 할당되지 않았습니다!");
        if (lr == null)
            Debug.LogError("LineRenderer가 할당되지 않았습니다!");
        else
            lr.positionCount = 2;

        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void LookAtMouse()
    {
        Vector2 mousePos = m_cam.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < transform.position.x && facingRight)
            Flip();
        else if (mousePos.x > transform.position.x && !facingRight)
            Flip();

        FireDirection = (mousePos - (Vector2)FirePoint.position).normalized;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = playerSprite.transform.localScale;
        scale.y *= -1;
        playerSprite.transform.localScale = scale;
    }

    private void TryFire()
    {
        if (Input.GetMouseButtonDown(1) && currentHook == null)
        {
            currentHook = Instantiate(HookPrefab, FirePoint.position, Quaternion.identity);
            Rigidbody2D hookRb = currentHook.GetComponent<Rigidbody2D>();
            if (hookRb != null)
            {
                hookRb.velocity = FireDirection * hookSpeed;
                hookRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            HookCollision hookCol = currentHook.AddComponent<HookCollision>();
            hookCol.Init(this);

            // 발사 직후 LR 활성화
            if (lr != null)
            {
                lr.enabled = true;
                lr.positionCount = 2;
            }
        }
    }

    public void OnHookHit(string tag, Vector2 hitPos)
    {
        hookTriggered = true;
        fixedHookPosition = hitPos;

        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }

        if (tag == "RB_Wall")
        {
            StopAllCoroutines();
            StartCoroutine(SnapPlayerToWall(hitPos));
        }
        else if (tag == "Reelbackable")
        {
            pullTarget = GameObject.FindWithTag("Reelbackable");
        }
        else if (tag == "Enemy")
        {
            EnemyGrapple eg = FindObjectOfType<EnemyGrapple>();
            if (eg != null)
            {
                isEnemyBeingGrappled = true;
                hookTriggered = false;
                eg.StartGrapple(GameObject.FindWithTag("Enemy"));
            }
        }
    }

    private IEnumerator SnapPlayerToWall(Vector2 hitPos)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) yield break;

        Vector2 startPos = player.transform.position;
        float elapsed = 0f;
        float duration = 0.3f;

        Vector2 wallOffset = new Vector2(facingRight ? -0.5f : 0.5f, 0);
        Vector2 targetPos = hitPos + wallOffset;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            player.transform.position = Vector2.Lerp(startPos, targetPos, elapsed / duration);

            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, FirePoint.position);
                lr.SetPosition(1, hitPos);
            }

            yield return null;
        }

        player.transform.position = targetPos;
        playerRb.velocity = Vector2.zero;
        playerRb.gravityScale = 0f;
        IsGrappling = true;

        if (lr != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, FirePoint.position);
            lr.SetPosition(1, hitPos);
        }

        yield return new WaitForSeconds(2f);
        StopGrapple();
    }

    public void StopGrapple()
    {
        if (!IsGrappling) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 2f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        IsGrappling = false;
        hookTriggered = false;

        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }
    }

    private void UpdateLine()
    {
        if (currentHook != null) // Hook이 존재하면 항상 LR 따라가기
        {
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, FirePoint.position);
                lr.SetPosition(1, currentHook.transform.position);
            }
            return;
        }

        if (hookTriggered)
        {
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, FirePoint.position);
                lr.SetPosition(1, fixedHookPosition);
            }
        }
        else
        {
            if (lr != null)
            {
                lr.enabled = false;
                lr.positionCount = 0;
            }
        }
    }

    private void HandlePullObject()
    {
        if (pullTarget != null && !isPullingObject)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isPullingObject = true;
                hookTriggered = false;

                Vector2 playerPos = FirePoint.position;
                Vector2 objPos = pullTarget.transform.position;
                Vector2 targetPos = objPos + (playerPos - objPos) * 0.5f;

                StartCoroutine(PullObjectCoroutine(pullTarget, targetPos));
            }
        }
    }

    private IEnumerator PullObjectCoroutine(GameObject obj, Vector2 targetPos)
    {
        while (Vector2.Distance(obj.transform.position, targetPos) > 0.01f)
        {
            obj.transform.position = Vector2.MoveTowards(obj.transform.position, targetPos, pullSpeed * Time.deltaTime);
            yield return null;
        }

        isPullingObject = false;
        pullTarget = null;

        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }
    }

    private void Update()
    {
        LookAtMouse();
        TryFire();
        UpdateLine();
        HandlePullObject();
    }
}
