using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] bool isMoving;

    [SerializeField] Vector2 input;
    [SerializeField] Animator animator;

    // LayerMask 이용해서 레이어로 못가는곳 지정하기.
    [SerializeField] LayerMask restrictedLayer;


    private static int moveRight = Animator.StringToHash("playerWalkRight");
    private static int moveLeft = Animator.StringToHash("playerWalkLeft");
    private static int moveUp = Animator.StringToHash("playerWalkUp");
    private static int moveDown = Animator.StringToHash("playerWalkDown");
    private static int idle = Animator.StringToHash("playerIdle");

    private static int runRight = Animator.StringToHash("playerRunRight");
    private static int runLeft = Animator.StringToHash("playerRunLeft");
    private static int runUp = Animator.StringToHash("playerRunUp");
    private static int runDown = Animator.StringToHash("playerRunDown");

    private static int curAniHash;
    private void pRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed += 5;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed -= 5;
        }
    }
    private void pMove()
    {

        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // no diagonal move
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));    
            }
        }
    }
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
    private void AnimatorPlay()
    {
        int checkAniHash;
        if (input.x > 0)
        {
            checkAniHash = moveRight;
            if (moveSpeed > 9.0f)
            {
                checkAniHash = runRight;
            }
        }
        else if (input.x < 0)
        {
            checkAniHash = moveLeft;
            if (moveSpeed > 9.0f)
            {
                checkAniHash = runLeft;
            }
        }
        else if (input.y > 0)
        {
            checkAniHash = moveUp;
            if (moveSpeed > 9.0f)
            {
                checkAniHash = runUp;
            }
        }
        else if (input.y < 0)
        {
            checkAniHash = moveDown;
            if (moveSpeed > 9.0f)
            {
                checkAniHash = runDown;
            }
        }
        else
        {
            checkAniHash = idle;
        }
        if (curAniHash != checkAniHash)
        {
            curAniHash = checkAniHash;
            animator.Play(curAniHash);
        }
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.3f, restrictedLayer) != null)
        {
            return false;
        }
        return true;
    }

    void Start()
    {
        
    }


    void Update()
    {
        pMove();
        pRun();
        AnimatorPlay();
    }
}
