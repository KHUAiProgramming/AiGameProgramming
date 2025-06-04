using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit " + other.name);
        }
    }
}
