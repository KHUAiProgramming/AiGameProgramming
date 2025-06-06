using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    Animator parentanim;
    private bool slashanimtriggered;
    Collider hitbox;

    // Start is called before the first frame update
    void Start()
    {
        parentanim = GetComponentInParent<Animator>();
        hitbox = GetComponent<Collider>();
        slashanimtriggered = false;
        hitbox.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (parentanim != null && !slashanimtriggered)
        {
            AnimatorStateInfo stateInfo = parentanim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Slash"))
            {
                slashanimtriggered = true;
                StartCoroutine(EnableColliderTemporarily());
            }
        }
    }

    IEnumerator EnableColliderTemporarily()
    {
        yield return new WaitForSeconds(0.5f);
        hitbox.enabled = true;
        yield return new WaitForSeconds(0.3f);
        hitbox.enabled = false;
        yield return new WaitForSeconds(1.0f);
        slashanimtriggered = false;
        
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
