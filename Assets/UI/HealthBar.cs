using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private MonoBehaviour character;

    private System.Func<float> hpPercentGetter;

    void Start()
    {
        if (healthSlider == null)
            Debug.LogError("HealthSlider not assigned!");

        if (character is DefenderController defender)
            hpPercentGetter = () => defender.HPPercentage;
        else if (character is AttackerController attacker)
            hpPercentGetter = () => attacker.HPPercentage;
        else
            Debug.LogError("Character must be DefenderController or AttackerController");
    }

    void Update()
    {
        if (hpPercentGetter != null)
        {
            healthSlider.value = hpPercentGetter();
        }
    }
}
