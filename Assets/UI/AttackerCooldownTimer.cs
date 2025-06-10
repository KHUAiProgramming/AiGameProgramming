using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AttackerCooldownTimer: MonoBehaviour
{
    [SerializeField] private Image[] skillCooldownOverlays;
    [SerializeField] private Text[] cooldownTexts;

    [SerializeField] private float[] cooldownTimes;

    public void StartCooldown(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= cooldownTimes.Length)
            return;

        StartCoroutine(CooldownRoutine(skillIndex, cooldownTimes[skillIndex]));
    }

    private IEnumerator CooldownRoutine(int index, float duration)
    {
        float timer = duration;

        if (skillCooldownOverlays[index] != null)
            skillCooldownOverlays[index].fillAmount = 1f;

        if (cooldownTexts[index] != null)
        {
            cooldownTexts[index].gameObject.SetActive(true);
            cooldownTexts[index].text = timer.ToString("F1");
        }

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (skillCooldownOverlays[index] != null)
                skillCooldownOverlays[index].fillAmount = timer / duration;

            if (cooldownTexts[index] != null)
                cooldownTexts[index].text = timer.ToString("F1");

            yield return null;
        }

        if (skillCooldownOverlays[index] != null)
            skillCooldownOverlays[index].fillAmount = 0f;

        if (cooldownTexts[index] != null)
        {
            cooldownTexts[index].gameObject.SetActive(false);
            cooldownTexts[index].text = "";
        }
    }
}