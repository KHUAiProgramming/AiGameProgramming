using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : MonoBehaviour
{
    public float MaxHP = 100.0f;
    public float CurrentHP;

    public float speed;
    // Start is called before the first frame update
    float hAxis;
    float vAxis;
    bool isWalkclicked;
    bool isJumpclicked;
    bool isDodgeClicked;
    bool isSlashclicked;
    bool isShieldClicked;

    bool isJump;
    bool isDodge;

    bool isSlashReady;
    bool isShieldReady;
    Vector3 moveVec;
    Rigidbody rb;
    Animator anim;


    public float dodgeCoolTime = 5f;
    private float dodgeCurrentCooldown = 0f;

    float slashDelay = 0f;
    public float slashCooldown = 2.5f;
    private float slashCurrentCooldown = 0f;

    public float shieldCooldown = 2.5f;
    private float shieldCurrentCooldown = 0f;
    

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        CurrentHP = MaxHP;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Dodge();
        Shield();

        if (dodgeCoolTime > 0f)
        {
            dodgeCurrentCooldown -= Time.deltaTime;
        }

        if (slashCurrentCooldown > 0f)
        {
            slashCurrentCooldown -= Time.deltaTime;
        }
        else
        {
            isSlashReady = true;
        }

        if (shieldCurrentCooldown > 0f)
        {
            shieldCurrentCooldown -= Time.deltaTime;
        }
        else
        {
            isShieldReady = true;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Slash"))
        {
            
        }
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        isWalkclicked = Input.GetButton("Walk");
        isJumpclicked = Input.GetButtonDown("Jump");
        isDodgeClicked = Input.GetButtonDown("Dodge");
        isSlashclicked = Input.GetButtonDown("Slash");
        isShieldClicked = Input.GetButtonDown("Shield");
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

    void Attack()
    {
        slashDelay += Time.deltaTime;
        if (isSlashclicked && isSlashReady && !isDodge)
        {
            anim.SetTrigger("DoSwing");
            slashCurrentCooldown = slashCooldown;
            isSlashReady = false;

            Invoke("ResetSlashReady", slashCooldown);
        }
    }
    void ResetSlashReady()
    {
        isSlashReady = true;
    }

    void Shield()
    {
        if (isShieldClicked && isShieldReady && !isDodge)
        {
            anim.SetTrigger("DoShield");
            shieldCurrentCooldown = shieldCooldown;
            isShieldReady = false;

            Invoke("ResetShieldReady", shieldCooldown);
        }
    }

    void ResetShieldReady()
    {
        isShieldReady = true;
    }


    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            isJump = false;
        }
    }

    public void GetDamage(float damage)
    {
        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            // 게임 끝
        }
    }
}
