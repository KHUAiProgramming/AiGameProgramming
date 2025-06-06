using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 10;
    private CapsuleCollider hitbox;

    void Start()
    {
        hitbox = GetComponent<CapsuleCollider>();
    }

    public void EnableHitbox()
    {
        Debug.Log("Enabling hitbox");
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit " + other.name);
        }
    }
}