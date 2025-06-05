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
    bool wDown;

    Vector3 moveVec;

    Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        CurrentHP = MaxHP;
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (wDown)
            transform.position += moveVec * speed * 0.5f * Time.deltaTime;
        else
            transform.position += moveVec * speed * Time.deltaTime;

        transform.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("IsRun", moveVec != Vector3.zero);
        anim.SetBool("IsWalk", wDown);

        transform.LookAt(transform.position + moveVec);
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
