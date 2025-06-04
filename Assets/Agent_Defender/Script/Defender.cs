using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    float hAxis;
    float vAxis;
    bool isWalkclicked;
    bool isJumpclicked;
    bool isDodgeClicked;
    bool isJump;
    bool isDodge;
    Vector3 moveVec;
    Rigidbody rb;
    Animator anim;

    public float dodgeCoolTime = 5f;
    private float dodgeCurrentCooldown = 0f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();

        if (dodgeCoolTime > 0f)
        {
            dodgeCurrentCooldown -= Time.deltaTime;
        }

    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        isWalkclicked = Input.GetButton("Walk");
        isJumpclicked = Input.GetButtonDown("Jump");
        isDodgeClicked = Input.GetButtonDown("Dodge");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isWalkclicked)
        {
            transform.position += moveVec * speed * 0.5f * Time.deltaTime;
        }

        else
        {
            transform.position += moveVec * speed * Time.deltaTime;
        }
            

        anim.SetBool("IsRun", moveVec != Vector3.zero);
        anim.SetBool("IsWalk", isWalkclicked);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if (isJumpclicked && !isJump)
        {
            rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
            anim.SetTrigger("DoJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (isDodgeClicked && moveVec != Vector3.zero && !isJump && dodgeCurrentCooldown <= 0f)
        {
            speed *= 2;
            anim.SetTrigger("DoDodge");
            isDodge = true;

            dodgeCurrentCooldown = dodgeCoolTime;

            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            isJump = false;
        }
    }
}
