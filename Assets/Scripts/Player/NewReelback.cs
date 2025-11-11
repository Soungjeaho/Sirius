using UnityEngine;
using System.Collections;

public class NewReelback : MonoBehaviour
{
    [Header("공통 설정")]
    [SerializeField] private SpriteRenderer playerSprite = null;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float hookSpeed = 20f;
    [SerializeField] public Transform FirePoint = null;
    [SerializeField] public LineRenderer lr = null;
    [SerializeField] private Rigidbody2D rb = null;

    [Header("일반 찌 프리팹 설정")]
    [SerializeField] private GameObject normalHookPrefab = null;

    [HideInInspector] public bool isEnemyBeingGrappled = false;

    private Vector2 FireDirection;
    private Camera m_cam = null;
    private bool facingRight = true;
    private GameObject currentHook = null;

    private bool hookTriggered = false;
    private Vector2 fixedHookPosition;

    [Header("Reelbackable 당기기 관련")]
    private GameObject pullTarget = null;
    private bool isPullingObject = false;
    [SerializeField] private float pullSpeed = 5f; // ✅ 당기는 속도

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
        if (m_cam == null || FirePoint == null)
            return;

        Vector3 mouseWorld = m_cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 mousePos = mouseWorld;
        FireDirection = (mousePos - (Vector2)FirePoint.position).normalized;

        if (mousePos.x < FirePoint.position.x && facingRight)
            Flip(false);
        else if (mousePos.x > FirePoint.position.x && !facingRight)
            Flip(true);
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        float yRotation = facingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void TryFire()
    {
        if (IsGrappling)
            return;

        if (Input.GetMouseButtonDown(1) && currentHook == null)
        {
            currentHook = Instantiate(normalHookPrefab, FirePoint.position, Quaternion.identity);

            Rigidbody2D hookRb = currentHook.GetComponent<Rigidbody2D>();
            if (hookRb != null)
            {
                hookRb.velocity = FireDirection * hookSpeed;
                hookRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            HookCollision hookCol = currentHook.AddComponent<HookCollision>();
            hookCol.Init(this);

            if (lr != null)
            {
                lr.enabled = true;
                lr.positionCount = 2;
            }

            StartCoroutine(CheckHookDistanceCoroutine(currentHook, FirePoint.position));
        }
    }

    private IEnumerator CheckHookDistanceCoroutine(GameObject hook, Vector2 startPos)
    {
        yield return null;

        while (hook != null)
        {
            float distance = Vector2.Distance(startPos, hook.transform.position);

            if (distance > maxDistance)
            {
                Destroy(hook);
                hook = null;

                if (lr != null)
                {
                    lr.enabled = false;
                    lr.positionCount = 0;
                }

                yield break;
            }

            yield return null;
        }
    }

    public void OnHookHit(string tag, Vector2 hitPos)
    {
        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }

        if (tag == "Enemy")
        {
            return;
        }

        if (tag == "RB_Wall")
        {
            hookTriggered = true;
            fixedHookPosition = hitPos;

            StopAllCoroutines();
            StartCoroutine(SnapPlayerToWall(hitPos));
        }
        else if (tag == "Reelbackable")
        {
            hookTriggered = true;
            fixedHookPosition = hitPos;

            // ✅ 맞은 오브젝트를 pullTarget으로 지정
            pullTarget = GameObject.FindWithTag("Reelbackable");
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
                rb.gravityScale = 1f;
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

    public void ResetHookState()
    {
        hookTriggered = false;

        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }
    }

    private void UpdateLine()
    {
        if (isEnemyBeingGrappled)
        {
            return;
        }

        if (currentHook != null)
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
                lr.enabled = true;
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

    private IEnumerator PullObjectRoutine()
    {
        if (pullTarget == null) yield break;
        isPullingObject = true;

        Rigidbody2D targetRb = pullTarget.GetComponent<Rigidbody2D>();
        Collider2D targetCol = pullTarget.GetComponent<Collider2D>();

        // 🔹 이동 전 설정
        if (targetRb != null)
        {
            targetRb.bodyType = RigidbodyType2D.Dynamic;
            targetRb.gravityScale = 0f;
            targetRb.velocity = Vector2.zero;
        }

        // ✅ ReelBackObjManager에서 volume 위치 가져오기
        ReelBackObjManager manager = FindObjectOfType<ReelBackObjManager>();
        Vector3 targetPos = (manager != null)
            ? manager.GetVolume().position
            : FirePoint.position;

        // ✅ Y좌표 고정 (수평 이동)
        targetPos.y = pullTarget.transform.position.y;

        // ✅ LineRenderer 초기화
        if (lr != null)
        {
            lr.enabled = true;
            lr.positionCount = 2;
        }

        // ✅ 일정 속도로 X축 이동
        while (pullTarget != null)
        {
            float dist = Mathf.Abs(pullTarget.transform.position.x - targetPos.x);
            if (dist <= 0.05f)
                break;

            // X축 방향 계산
            Vector2 dir = new Vector2(Mathf.Sign(targetPos.x - pullTarget.transform.position.x), 0f);
            Vector2 moveStep = dir * pullSpeed * Time.deltaTime;

            if (targetRb != null && targetRb.bodyType == RigidbodyType2D.Dynamic)
                targetRb.MovePosition(targetRb.position + moveStep);
            else
                pullTarget.transform.position += (Vector3)moveStep;

            // ✅ LineRenderer 갱신
            if (lr != null)
            {
                lr.SetPosition(0, FirePoint.position);
                lr.SetPosition(1, pullTarget.transform.position);
            }

            yield return null;
        }

        // ✅ 도착 즉시 LineRenderer 비활성화
        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }

        // ✅ Grapple 상태 초기화
        hookTriggered = false;
        fixedHookPosition = Vector2.zero;

        // ✅ 도착 후 오브젝트 고정 (Static으로 복구)
        if (targetRb != null)
        {
            targetRb.velocity = Vector2.zero;
            targetRb.gravityScale = 1f;
            targetRb.bodyType = RigidbodyType2D.Static;
        }

        // ✅ Collider 복원
        if (targetCol != null)
            targetCol.isTrigger = false;

        // ✅ 태그 변경
        pullTarget.tag = "Obstacle";
        isPullingObject = false;
        pullTarget = null;
    }

    private void Update()
    {
        LookAtMouse();
        TryFire();
        UpdateLine();

        // ✅ E키로 오브젝트 당기기 시작
        if (Input.GetKeyDown(KeyCode.E) && pullTarget != null && !isPullingObject)
        {
            StartCoroutine(PullObjectRoutine());
        }
    }
}
