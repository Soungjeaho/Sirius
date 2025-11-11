using UnityEngine;
using System.Collections;

public class HeavyFloat : MonoBehaviour
{
    [Header("무거운 찌 설정")]
    [SerializeField] private GameObject heavyHookPrefab = null;
    [SerializeField] private Transform firePoint = null;
    [SerializeField] private float heavySpeed = 12f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private int gaugeCost = 3;
    [SerializeField] private float recoilForce = 4f;

    [Header("참조")]
    [SerializeField] private LineRenderer lr = null;
    [SerializeField] private EnergyGauge gauge = null;
    [SerializeField] private Rigidbody2D playerRb = null;

    private Camera m_cam;
    private GameObject currentHook;
    private Vector2 fireDirection;
    private bool isFired = false;
    private bool facingRight = true;

    private void Start()
    {
        m_cam = Camera.main;

        if (firePoint == null)
            Debug.LogError("FirePoint가 할당되지 않았습니다!");
        if (gauge == null)
            gauge = FindObjectOfType<EnergyGauge>();
        if (playerRb == null)
            playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        AimAtMouse();

        // 🔹 발사 (우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            TryFireHeavy();
        }

        // 🔹 회수 (E키)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryRecallHeavy();
        }

        UpdateLine();
    }

    private void AimAtMouse()
    {
        if (m_cam == null || firePoint == null) return;

        Vector2 mousePos = m_cam.ScreenToWorldPoint(Input.mousePosition);
        fireDirection = (mousePos - (Vector2)firePoint.position).normalized;

        // 플레이어 방향 전환
        if (mousePos.x < transform.position.x && facingRight)
        {
            Flip(false);
        }
        else if (mousePos.x > transform.position.x && !facingRight)
        {
            Flip(true);
        }
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        float yRotation = facingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void TryFireHeavy()
    {
        if (isFired) return;

        // 🔹 게이지는 발사 시 소모하지 않음

        // HeavyFloatProjectile 인스턴스 생성
        currentHook = Instantiate(heavyHookPrefab, firePoint.position, Quaternion.identity);

        // FirePoint 주입
        HeavyFloatProjectile projectile = currentHook.GetComponent<HeavyFloatProjectile>();
        if (projectile != null)
        {
            projectile.SetFirePoint(firePoint);
            projectile.SetGaugeReference(gauge, gaugeCost); // ✅ 게이지 참조 전달 (적 충돌 시 차감)
        }

        // 발사 속도 부여
        Rigidbody2D hookRb = currentHook.GetComponent<Rigidbody2D>();
        if (hookRb != null)
        {
            hookRb.velocity = fireDirection * heavySpeed;
            hookRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // 플레이어 반동
        if (playerRb != null)
        {
            playerRb.AddForce(-fireDirection * recoilForce, ForceMode2D.Impulse);
        }

        // 라인렌더러 표시
        if (lr != null)
        {
            lr.enabled = true;
            lr.positionCount = 2;
        }

        StartCoroutine(CheckDistanceCoroutine(currentHook, firePoint.position));
        isFired = true;
    }

    private void TryRecallHeavy()
    {
        if (!isFired || currentHook == null) return;

        // 🔹 Switch 위에 있는 경우만 회수 허용
        HeavyFloatProjectile projectile = currentHook.GetComponent<HeavyFloatProjectile>();
        if (projectile != null && projectile.IsOnSwitch)
        {
            Destroy(currentHook);
            currentHook = null;
            isFired = false;

            if (lr != null)
            {
                lr.enabled = false;
                lr.positionCount = 0;
            }

            Debug.Log("E키 입력으로 찌 회수 완료");
        }
    }

    private IEnumerator CheckDistanceCoroutine(GameObject hook, Vector2 startPos)
    {
        while (hook != null)
        {
            float dist = Vector2.Distance(startPos, hook.transform.position);
            if (dist > maxDistance)
            {
                Destroy(hook);
                hook = null;

                if (lr != null)
                {
                    lr.enabled = false;
                    lr.positionCount = 0;
                }

                isFired = false;
                yield break;
            }
            yield return null;
        }

        isFired = false;
    }

    private void UpdateLine()
    {
        if (currentHook != null)
        {
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, firePoint.position);
                lr.SetPosition(1, currentHook.transform.position);
            }
        }
        else if (lr != null && lr.enabled)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }
    }
}
