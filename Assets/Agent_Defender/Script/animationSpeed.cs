using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationSpeed : MonoBehaviour
{
    Animator animator;
    public float Speed = 1.0f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("Speed", Mathf.Clamp(Speed, 0.1f, 3f));
    }
}