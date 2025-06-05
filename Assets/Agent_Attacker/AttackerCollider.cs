using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class AttackerCollider : MonoBehaviour
{
    Animator animator;
    Collider mycollider;
    private bool attackanimtriggered;



    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInParent<Animator>();
        mycollider = GetComponent<Collider>();
        mycollider.enabled = false;
        attackanimtriggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("") && !attackanimtriggered)
            {
                attackanimtriggered = true;
                StartCoroutine(EnableColliderTemporarily());
            }
            
        }
        
    }

    IEnumerator EnableColliderTemporarily()
    {
        yield return new WaitForSeconds(0.7f);
        mycollider.enabled = true;
        yield return new WaitForSeconds(0.2f);
        mycollider.enabled = false;
        yield return new WaitForSeconds(0.3f);
        attackanimtriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{name}, 이 안에 자유롭게 텍스트 입력 가능{other.gameObject.name}");
        var collisionobject = other.gameObject.GetComponent<Defender>();
        if (collisionobject != null)
        {
            collisionobject.GetDamage(10);
        }

    }
}
