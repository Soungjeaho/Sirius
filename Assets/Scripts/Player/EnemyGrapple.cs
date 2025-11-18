using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 그랩(릴백)로 적을 끌어오고 파괴하는 로직.
/// - 게이지 차감/회복
/// - 적의 이동/시야 컴포넌트 비활성화
/// - Rigidbody2D 정지 및 Kinematic 전환(끝나면 복귀 가능)
/// - 라인렌더러 업데이트
/// </summary>
public class EnemyGrapple : MonoBehaviour
{
    [SerializeField] private NewReelback reelback;
    [SerializeField] private EnergyGauge gauge;
    [SerializeField] private float grappleSpeed = 5f;
    [SerializeField] private float disappearDelay = 0.5f;
    [SerializeField] private int gaugeCost = 4;

    private bool isPullingEnemy = false;

    // 잠금/복구를 위한 캐시
    private class LockedEnemy
    {
        public GameObject go;
        public Rigidbody2D rb;
        public RigidbodyType2D prevBodyType;
        public float prevAngularVel;
        public Vector2 prevVelocity;
        public List<(Behaviour comp, bool wasEnabled)> toggled = new List<(Behaviour, bool)>();
        public Animator animator;
        public float prevAnimSpeed = 1f;

        // EnemyMovement / RangedAttackVision 등 네임스페이스 몰라도 이름으로 비활성화
        static readonly HashSet<string> targetBehaviourNames = new HashSet<string>{
            "EnemyMovement","RangedAttackVision"
        };

        public LockedEnemy(GameObject go) { this.go = go; }

        public void Lock()
        {
            if (!go) return;

            // 1) 움직임/시야 비헤이비어 비활성화
            var behaviours = go.GetComponents<Behaviour>();
            foreach (var b in behaviours)
            {
                if (b == null) continue;
                var name = b.GetType().Name;
                if (targetBehaviourNames.Contains(name))
                {
                    toggled.Add((b, b.enabled));
                    b.enabled = false;
                }
            }

            // 2) 애니메이터 정지(선택) – 당겨오는 중에 포즈 고정
            animator = go.GetComponentInChildren<Animator>();
            if (animator)
            {
                prevAnimSpeed = animator.speed;
                animator.speed = 0f;
            }

            // 3) 물리 정지 및 반응 중단
            rb = go.GetComponent<Rigidbody2D>();
            if (rb)
            {
                prevVelocity = rb.velocity;
                prevAngularVel = rb.angularVelocity;
                prevBodyType = rb.bodyType;

                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic; // isKinematic 대신 최신식
            }
        }

        public void Unlock() // 적을 파괴하지 않고 되돌릴 때 사용
        {
            if (!go) return;

            // 비헤이비어 원복
            foreach (var (comp, wasEnabled) in toggled)
            {
                if (comp) comp.enabled = wasEnabled;
            }
            toggled.Clear();

            // 애니메이터 속도 복구
            if (animator) animator.speed = prevAnimSpeed;

            // 물리 복구
            if (rb)
            {
                rb.bodyType = prevBodyType;
                rb.velocity = prevVelocity;
                rb.angularVelocity = prevAngularVel;
            }
        }
    }

    private LockedEnemy _lock; // 현재 잠그는 적 정보 캐시

    public void StartGrapple(GameObject enemy)
    {
        if (isPullingEnemy || enemy == null) return;

        // 1) 에너지 확인
        if (!gauge || !gauge.UseGauge(gaugeCost))
        {
            Debug.Log("Energy 부족! (또는 gauge 미할당)");
            return;
        }

        // 2) 적 잠금(이동/시야/애니/물리)
        _lock = new LockedEnemy(enemy);
        _lock.Lock();

        // 3) 상태 플래그
        isPullingEnemy = true;
        if (reelback) reelback.isEnemyBeingGrappled = true; // LR/입력 상호작용 방지

        // 4) 끌어오기 코루틴
        StartCoroutine(PullEnemyCoroutine(enemy));
    }

    private IEnumerator PullEnemyCoroutine(GameObject enemy)
    {
        if (!reelback)
        {
            Debug.LogWarning("reelback 미할당");
            yield break;
        }

        Transform enemyTransform = enemy ? enemy.transform : null;
        Transform playerTransform = reelback.FirePoint;

        // 라인렌더러 초기화
        if (reelback.lr != null)
        {
            reelback.lr.enabled = true;
            reelback.lr.positionCount = 2;
        }

        // 적이 존재하고, 플레이어 FirePoint에 도달할 때까지
        while (enemyTransform && playerTransform && !HasReachedFirePoint(enemyTransform, playerTransform))
        {
            enemyTransform.position = Vector2.MoveTowards(
                enemyTransform.position,
                playerTransform.position,
                grappleSpeed * Time.deltaTime
            );

            if (reelback.lr != null)
            {
                reelback.lr.positionCount = 2;
                reelback.lr.SetPosition(0, playerTransform.position);
                reelback.lr.SetPosition(1, enemyTransform.position);
            }

            yield return null;
        }

        // 잠깐 연출 대기
        yield return new WaitForSeconds(disappearDelay);

        // 적 파괴(원하면 이 부분 주석 처리하고 _lock.Unlock() 호출로 생존시킬 수 있음)
        if (enemy) Destroy(enemy);

        // 게이지 환급
        if (gauge) gauge.AddGauge(1);

        // 라인렌더러 닫기
        if (reelback.lr != null)
        {
            reelback.lr.enabled = false;
            reelback.lr.positionCount = 0;
        }

        // 상태 해제
        if (reelback) reelback.isEnemyBeingGrappled = false;
        isPullingEnemy = false;

        // 적을 살려두는 설계로 바꾸고 싶다면:
        // if (_lock != null) _lock.Unlock();
        _lock = null;
    }

    private bool HasReachedFirePoint(Transform enemy, Transform firePoint)
    {
        float distance = Vector2.Distance(enemy.position, firePoint.position);
        return distance <= 0.3f;
    }

    /// <summary>
    /// 중간 취소(예: 피격/입력) 시 호출하면 안전하게 원복합니다.
    /// </summary>
    public void CancelGrapple()
    {
        // 라인 닫기
        if (reelback && reelback.lr != null)
        {
            reelback.lr.enabled = false;
            reelback.lr.positionCount = 0;
        }

        if (reelback) reelback.isEnemyBeingGrappled = false;
        isPullingEnemy = false;

        // 적 원복
        if (_lock != null) _lock.Unlock();
        _lock = null;
    }
}
