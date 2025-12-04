using Project.Combat;
using System.Collections;
using UnityEngine;

public class Reelback : MonoBehaviour
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

    private Vector2 fireDirection;
    private Camera m_cam = null;
    private GameObject currentHook = null;

    private bool hookTriggered = false;
    private Vector2 fixedHookPosition;

    [Header("Reelbackable 당기기 관련")]
    [SerializeField] private float pullSpeed = 10f;
    [SerializeField] private float stopDistance = 0.1f;

    [Header("Enemy Hook 설정")]
    [SerializeField] private EnergyGauge energyGauge = null;
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private int enemyHookGaugeCost = 4;
    [SerializeField] private float enemyPullSpeed = 12f;
    [SerializeField] private float enemyStopDistance = 0.3f;
    [SerializeField] private Vector2 enemyPullOffset = new Vector2(1f, 0f);

    private bool isPullingEnemy = false;
    private GameObject pullTarget = null;       // 실제로 맞은 Block
    private Transform pullTargetVolume = null;  // Block과 같은 부모의 Volume
    private bool isPullingObject = false;

    public bool IsGrappling { get; private set; } = false;

    private void Start()
    {
        m_cam = Camera.main;

        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        LookAtMouse();
        TryFire();
        UpdateLine();

        // E 키로 Block 당기기 시작
        if (Input.GetKeyDown(KeyCode.E) && pullTarget != null && !isPullingObject)
        {
            StartCoroutine(PullObjectRoutine());
        }
    }

    private void LookAtMouse()
    {
        if (m_cam == null || FirePoint == null)
        {
            return;
        }

        Vector3 mouseWorld = m_cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 mousePos = mouseWorld;
        fireDirection = (mousePos - (Vector2)FirePoint.position).normalized;
    }

    private void TryFire()
    {
        if (IsGrappling)
        {
            return;
        }

        // 마우스 Y가 FirePoint 기준 범위 밖이면 발사하지 않음
        if (PlayerController.Instance != null &&
            !PlayerController.Instance.IsMouseInFrontOfPlayer())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1) && currentHook == null)
        {
            currentHook = Instantiate(normalHookPrefab, FirePoint.position, Quaternion.identity);

            Rigidbody2D hookRb = currentHook.GetComponent<Rigidbody2D>();
            if (hookRb != null)
            {
                hookRb.velocity = fireDirection * hookSpeed;
                hookRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                hookRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            HookCollision hookCol = currentHook.GetComponent<HookCollision>();
            if (hookCol != null)
            {
                hookCol.Init(this);
            }

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

    // Hook이 어떤 오브젝트에 맞았는지 여기로 전달받음
    public void OnHookHit(GameObject hitObject, Vector2 hitPos)
    {
        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }

        string tag = hitObject.tag;

        if (tag == "Enemy")
        {
            // Heavy Hook면 이 기능 안 씀
            if (playerController != null && !playerController.IsNormalHook)
            {
                return;
            }

            // 이미 다른 Enemy를 끌고 있는 중이면 무시
            if (isPullingEnemy)
            {
                return;
            }

            // 게이지 부족하면 아무 일도 안 일어나게
            if (energyGauge != null && !energyGauge.UseGauge(enemyHookGaugeCost))
            {
                Debug.Log("게이지 부족! Enemy Hook 사용 불가.");
                return;
            }

            IDamageable dmg = hitObject.GetComponentInParent<IDamageable>();
            if (dmg == null || dmg.IsDead)
            {
                return;
            }

            // Enemy를 먼저 플레이어 쪽으로 끌고 온 다음, 끝나면 즉사 처리
            StartCoroutine(PullEnemyThenKill(hitObject, dmg));

            return;
        }

        if (tag == "RB_Wall")
        {
            hookTriggered = true;
            fixedHookPosition = hitPos;

            StopAllCoroutines();
            StartCoroutine(SnapPlayerToWall(hitPos));
            return;
        }

        if (tag == "Reelbackable")
        {
            hookTriggered = true;
            fixedHookPosition = hitPos;

            pullTarget = hitObject;

            pullTargetVolume = null;
            Transform parent = hitObject.transform.parent;
            if (parent != null)
            {
                Transform volume = parent.Find("Volume");
                if (volume != null)
                {
                    pullTargetVolume = volume;
                }
            }

            if (pullTargetVolume == null)
            {
                Debug.LogWarning("Reelbackable의 부모에서 'Volume'을 찾지 못했습니다. fixedHookPosition을 목표로 사용합니다.");
            }
        }
    }

    private IEnumerator SnapPlayerToWall(Vector2 hitPos)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            yield break;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null)
        {
            yield break;
        }

        Vector2 startPos = player.transform.position;
        float elapsed = 0f;
        float duration = 0.3f;

        //  PlayerController 방향 기준으로 벽 옆에 붙이기
        bool faceRight = true;

        if (playerController != null)
        {
            faceRight = playerController.FacingRight;
        }
        else if (playerSprite != null)
        {
            // flipX == true 면 왼쪽을 보고 있는 상태
            faceRight = !playerSprite.flipX;
        }

        Vector2 wallOffset = new Vector2(faceRight ? -0.5f : 0.5f, 0f);
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

        yield return new WaitForSeconds(2f);
        StopGrapple();
    }

    public void StopGrapple()
    {
        if (!IsGrappling)
        {
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.gravityScale = 1f;
                rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
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
        if (isEnemyBeingGrappled)
        {
            return;
        }

        if (currentHook != null)
        {
            if (lr != null)
            {
                lr.enabled = true;
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

    // Reelbackable 오브젝트를 수평으로 끌어오는 루틴
    private IEnumerator PullObjectRoutine()
    {
        if (pullTarget == null)
        {
            yield break;
        }

        isPullingObject = true;

        Rigidbody2D targetRb = pullTarget.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.bodyType = RigidbodyType2D.Dynamic;
            targetRb.gravityScale = 0f;
            targetRb.velocity = Vector2.zero;
        }

        // 목표 위치: Volume이 있으면 그 위치, 없으면 fixedHookPosition 사용
        Vector3 targetPos;
        if (pullTargetVolume != null)
        {
            targetPos = pullTargetVolume.position;
        }
        else
        {
            targetPos = fixedHookPosition;
        }

        // 수평 이동만: Y는 Block의 현재 Y 유지
        targetPos.y = pullTarget.transform.position.y;

        if (lr != null)
        {
            lr.enabled = true;
            lr.positionCount = 2;
        }

        while (pullTarget != null)
        {
            float dist = Mathf.Abs(pullTarget.transform.position.x - targetPos.x);
            if (dist <= stopDistance)
            {
                break;
            }

            float dirX = Mathf.Sign(targetPos.x - pullTarget.transform.position.x);
            Vector2 moveStep = new Vector2(dirX, 0f) * pullSpeed * Time.deltaTime;

            if (targetRb != null)
            {
                targetRb.MovePosition(targetRb.position + moveStep);
            }
            else
            {
                pullTarget.transform.position += (Vector3)moveStep;
            }

            if (lr != null)
            {
                lr.SetPosition(0, FirePoint.position);
                lr.SetPosition(1, pullTarget.transform.position);
            }

            yield return null;
        }

        // 도착 후 상태 정리
        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }

        hookTriggered = false;
        fixedHookPosition = Vector2.zero;

        if (targetRb != null)
        {
            targetRb.velocity = Vector2.zero;
            targetRb.gravityScale = 1f;
            targetRb.bodyType = RigidbodyType2D.Static;
        }

        // 태그 Obstacle로 변경
        pullTarget.tag = "Obstacle";

        pullTarget = null;
        pullTargetVolume = null;
        isPullingObject = false;
    }

    private IEnumerator PullEnemyThenKill(GameObject enemyObj, IDamageable dmg)
    {
        if (enemyObj == null)
        {
            yield break;
        }

        isEnemyBeingGrappled = true;
        isPullingEnemy = true;

        Rigidbody2D enemyRb = enemyObj.GetComponent<Rigidbody2D>();
        float originalGravity = 0f;

        if (enemyRb != null)
        {
            originalGravity = enemyRb.gravityScale;
            enemyRb.gravityScale = 0f;
            enemyRb.velocity = Vector2.zero;
        }

        Transform playerTr = playerController != null ? playerController.transform : null;

        if (lr != null)
        {
            lr.enabled = true;
            lr.positionCount = 2;
        }

        while (enemyObj != null && playerTr != null)
        {
            Vector2 baseOffset = enemyPullOffset;

            // 플레이어 스프라이트 방향에 따라 좌우 반전
            if (playerController != null && !playerController.FacingRight)
            {
                baseOffset.x *= -1f;
            }

            Vector2 targetPos = (Vector2)playerTr.position + baseOffset;
            Vector2 currentPos = enemyObj.transform.position;

            if (Vector2.Distance(currentPos, targetPos) <= enemyStopDistance)
            {
                break;
            }

            Vector2 dir = (targetPos - currentPos).normalized;
            Vector2 moveStep = dir * enemyPullSpeed * Time.deltaTime;

            if (enemyRb != null)
            {
                enemyRb.MovePosition(currentPos + moveStep);
            }
            else
            {
                enemyObj.transform.position = currentPos + moveStep;
            }

            if (lr != null)
            {
                lr.SetPosition(0, FirePoint.position);
                lr.SetPosition(1, enemyObj.transform.position);
            }

            yield return null;
        }

        // 라인 꺼주기
        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }

        // 중력 원상복구
        if (enemyRb != null)
        {
            enemyRb.gravityScale = originalGravity;
        }

        // 아직 안 죽어있다면 여기서 즉사 처리
        if (!dmg.IsDead)
        {
            Vector2 hitPoint = enemyObj != null ? (Vector2)enemyObj.transform.position : (Vector2)playerTr.position;
            Vector2 hitNormal = Vector2.right;

            if (playerController != null && !playerController.FacingRight)
            {
                hitNormal = Vector2.left;
            }
            dmg.ApplyDamage(9999, hitPoint, hitNormal, this);
            Debug.Log("Enemy Hook 즉사 처리!");
        }

        // 처치에 성공했다면 게이지 +1
        if (dmg.IsDead && energyGauge != null)
        {
            energyGauge.AddGauge(1);
        }

        isEnemyBeingGrappled = false;
        isPullingEnemy = false;
    }
}
