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
    private int hashAttack; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();

        hashVelocityX = Animator.StringToHash("velocityX");
        hashVelocityY = Animator.StringToHash("velocityY");
        hashIsJump = Animator.StringToHash("isJump");
        hashAttack = Animator.StringToHash("attack"); 
    }

    private void Update()
    {
        if (animator == null || playerController == null)
        {
            return;
        }

        float input = Mathf.Abs(playerController.HorizontalInput);
        float value = playerController.IsRunning ? 1f : (input > 0f ? 0.5f : 0f);
        animator.SetFloat(hashVelocityX, value);

        float currentVelocityY = playerController.GetRigidbody().velocity.y;
        animator.SetFloat(hashVelocityY, currentVelocityY);

        bool isCurrentlyJumpingOrFalling = !playerController.Grounded();
        animator.SetBool(hashIsJump, isCurrentlyJumpingOrFalling);

        // --- 공격 Trigger 발동 (추가됨) ---
        if (playerController.TriggerAttack)
        {
            animator.SetTrigger(hashAttack);
        }
    }
}