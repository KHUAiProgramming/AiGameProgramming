using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    public GameObject A;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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
