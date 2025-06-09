using Unity.MLAgents;
using UnityEngine;

public class Academy : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 20f;
        Application.targetFrameRate = -1;
    }
}