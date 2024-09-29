using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{

    // [SerializeField] Rigidbody2D rigid;
    // private float x;
    // private float y;


    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] bool isMoving;

    [SerializeField] Vector2 input;
    [SerializeField] Animator animator;

    // LayerMask 이용해서 레이어로 못가는곳 지정하기.
    [SerializeField] LayerMask restrictedLayer;
    [SerializeField] LayerMask grassLayer;



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


            // 맵에서 점프로 넘어가는 부분에서 플랫포머를 쓰려는데 리지드 바디가 없어서 안가지는데
            // 위에 움직이는 방식을 바꾸거나 다른방식으로 넘어가는거를 해야되거나 함.
            //   x = Input.GetAxisRaw("Horizontal");
            //   y = Input.GetAxisRaw("Vertical");
            //   rigid.AddForce(Vector2.right * x * moveSpeed, ForceMode2D.Force);
            //   rigid.AddForce(Vector2.up * y * moveSpeed, ForceMode2D.Force);



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

        CheckForEncounters();

    }
    private void CheckForEncounters()
    {
        // TileCollider에 CompositeCollider만쓰면 안된다 ... 다시된다...
        
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            // 1~100까지 생성 하는수에서 50보다 낮으면 걸리는건데 너무 안걸리는데. 50:50인건데
            // 걸릴떈 또 확걸린다. 좀 뭔가 그럴사한 확률을 만들어야될거같다.
            if (Random.Range(1, 101) <= 50) 
            {
                Debug.Log("야생의 포켓몬이 튀어나왔다!");
            }
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
