using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderSword : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 공격 애니메이션이 시작했을 때만 충돌 활성화화
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 다른 컴포넌트의 GetDamage를 호출
        var otherobject = other.gameObject;
        //otherobject.GetComponent<AttackerManager>.GetDamage();
        Debug.Log($"{this.gameObject.name}collider checked{other.gameObject.name}");
    }
}
