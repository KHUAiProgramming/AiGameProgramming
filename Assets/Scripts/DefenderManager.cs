using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderManager : MonoBehaviour
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
}
