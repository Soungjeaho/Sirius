using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    private int hashVelocityX;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();

        hashVelocityX = Animator.StringToHash("velocityX");
    }

    private void Update()
    {
        if (animator == null || playerController == null)
        {
            return;
        }

        float input = Mathf.Abs(playerController.HorizontalInput);
        float value = 0f;

        // 입력 없으면 Idle → 0
        if (input <= 0f)
        {
            value = 0f;
        }
        else
        {
            // 입력 있음 → 걷기 / 달리기 구분
            if (playerController.IsRunning)
            {
                // Run
                value = 1f;
            }
            else
            {
                // Walk
                value = 0.5f;
            }
        }

        animator.SetFloat(hashVelocityX, value);
    }
}
