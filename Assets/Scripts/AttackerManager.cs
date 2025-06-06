using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerManager : MonoBehaviour
{
    public float maxHp = 100.0f;
    public float damage = 10.0f;
    public float currentHp;

    // Start is called before the first frame update
    void Awake()
    {
        currentHp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetDamage(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            
            // 사망 애니메이션 (현재는 오브젝트 비활성화)
            gameObject.SetActive(false);
        }
    }
}
