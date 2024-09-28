using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;

    [SerializeField] bool isMoving;

    private void pMove()
    {
        Vector2 input;

        if (isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
