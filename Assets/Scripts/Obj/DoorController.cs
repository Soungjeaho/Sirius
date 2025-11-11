using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("오브젝트 참조")]
    [SerializeField] private Transform switchPlate;
    [SerializeField] private Transform switchCase;
    [SerializeField] private Transform doorSprite;
    [SerializeField] private Transform doorOrigin;
    [SerializeField] private Transform doorTarget;

    [Header("설정")]
    [SerializeField] private float doorMoveTime = 1.0f;    // 문 이동 시간
    [SerializeField] private float switchMoveTime = 0.3f;  // 스위치 눌리고 올라오는 속도
    [SerializeField] private float doorCloseDelay = 3.0f;  // 스위치 해제 후 닫히는 딜레이

    private bool isOpening = false;
    private bool isPressed = false;
    private Coroutine doorCloseCoroutine;
    private Vector3 switchOriginPos;

    private void Start()
    {
        switchOriginPos = switchPlate.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPressed && (collision.CompareTag("Player") || collision.CompareTag("HeavyProjectile")))
        {
            isPressed = true;

            // 스위치 내려가기
            StartCoroutine(PressSwitchRoutine(true));

            // 닫히는 코루틴이 돌고 있으면 취소
            if (doorCloseCoroutine != null)
            {
                StopCoroutine(doorCloseCoroutine);
                doorCloseCoroutine = null;
            }

            // 문 열기
            StartCoroutine(OpenDoorRoutine(true));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isPressed && (collision.CompareTag("Player") || collision.CompareTag("HeavyProjectile")))
        {
            isPressed = false;

            // 스위치 올라가기
            StartCoroutine(PressSwitchRoutine(false));

            // 3초 카운트 후 닫기 시작 (중복 방지)
            if (doorCloseCoroutine == null)
                doorCloseCoroutine = StartCoroutine(CloseDoorAfterDelay());
        }
    }

    private IEnumerator PressSwitchRoutine(bool pressed)
    {
        Vector3 start = switchPlate.position;

        // 스위치가 정확히 switchCase 위치까지 이동
        Vector3 end = pressed ? switchCase.position : switchOriginPos;

        float elapsed = 0f;
        while (elapsed < switchMoveTime)
        {
            elapsed += Time.deltaTime;
            switchPlate.position = Vector3.Lerp(start, end, elapsed / switchMoveTime);
            yield return null;
        }

        switchPlate.position = end;
    }


    private IEnumerator OpenDoorRoutine(bool open)
    {
        if (isOpening)
            yield break;

        isOpening = true;

        float elapsed = 0f;
        Vector3 startPos = doorSprite.position;
        Vector3 endPos = open ? doorTarget.position : doorOrigin.position;

        while (elapsed < doorMoveTime)
        {
            elapsed += Time.deltaTime;
            doorSprite.position = Vector3.Lerp(startPos, endPos, elapsed / doorMoveTime);
            yield return null;
        }

        doorSprite.position = endPos;
        isOpening = false;
    }

    private IEnumerator CloseDoorAfterDelay()
    {
        // 🔹 스위치에서 발 뗀 후 3초 기다림
        yield return new WaitForSeconds(doorCloseDelay);

        // 🔹 기다리는 도중 다시 눌리면 닫지 않음
        if (isPressed)
        {
            doorCloseCoroutine = null;
            yield break;
        }

        // 🔹 3초 지난 후 문 닫기
        StartCoroutine(OpenDoorRoutine(false));
        doorCloseCoroutine = null;
    }
}
