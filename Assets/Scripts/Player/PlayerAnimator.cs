using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    private int hashVelocityX;
    private int hashVelocityY;
    private int hashIsJump;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();

        hashVelocityX = Animator.StringToHash("velocityX");
        hashVelocityY = Animator.StringToHash("velocityY");
        hashIsJump = Animator.StringToHash("isJump");
    }

    private void Update()
    {
        if (animator == null || playerController == null)
        {
            return;
        }

        // ... (기존 velocityX, velocityY, isJump 로직은 유지) ...

        float input = Mathf.Abs(playerController.HorizontalInput);
        float value = playerController.IsRunning ? 1f : (input > 0f ? 0.5f : 0f);
        animator.SetFloat(hashVelocityX, value);

        float currentVelocityY = playerController.GetRigidbody().velocity.y;
        animator.SetFloat(hashVelocityY, currentVelocityY);

        bool isCurrentlyJumpingOrFalling = !playerController.Grounded();
        animator.SetBool(hashIsJump, isCurrentlyJumpingOrFalling);
    }
}