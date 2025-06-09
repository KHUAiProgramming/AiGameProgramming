using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenderCooldownTimer : MonoBehaviour
{
    [SerializeField] private Image[] skillCooldownOverlays;
    [SerializeField] private Text[] cooldownTexts;
    [SerializeField] private float[] cooldownTimes;

    private float[] cooldownTimers;

    void Start()
    {
        cooldownTimers = new float[cooldownTimes.Length];

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            cooldownTimers[i] = 0f;
            skillCooldownOverlays[i].fillAmount = 0f;
            cooldownTexts[i].gameObject.SetActive(false);
        }
    }

    public void StartCooldown(int index)
    {
        if (index >= 0 && index < cooldownTimers.Length)
        {
            cooldownTimers[index] = cooldownTimes[index];
            cooldownTexts[index].gameObject.SetActive(true);
        }
    }

    void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;

                float ratio = cooldownTimers[i] / cooldownTimes[i];
                skillCooldownOverlays[i].fillAmount = ratio;

                cooldownTexts[i].text = cooldownTimers[i].ToString("F1");

                if (cooldownTimers[i] <= 0f)
                {
                    cooldownTimers[i] = 0f;
                    skillCooldownOverlays[i].fillAmount = 0f;
                    cooldownTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }
}