using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [Header("Combat Participants (미리 배치된 오브젝트)")]
    [SerializeField] private AttackerController attacker;
    [SerializeField] private DefenderController defender;

    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Slider attackerHPBar;
    [SerializeField] private Slider defenderHPBar;
    [SerializeField] private Text resultText;
    [SerializeField] private Text attackerHPText;
    [SerializeField] private Text defenderHPText;

    [Header("Combat Settings")]
    [SerializeField] private float combatTimeLimit = 120f; // 2분 제한

    private bool combatInProgress = false;
    private bool combatEnded = false;
    private float combatTimer = 0f;

    public bool CombatInProgress => combatInProgress;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartCombat);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetCombat);

        if (resultText != null)
            resultText.text = "전투 시작 버튼을 눌러주세요";
    }

    void Update()
    {
        if (combatInProgress && !combatEnded)
        {
            UpdateCombat();
            UpdateUI();
        }
    }

    public void StartCombat()
    {
        if (combatInProgress) return;

        // 전투 초기화
        combatInProgress = true;
        combatEnded = false;
        combatTimer = 0f;

        // HP 리셋
        if (attacker != null) attacker.ResetHP();
        if (defender != null) defender.ResetHP();

        if (resultText != null)
            resultText.text = "전투 진행 중...";

        if (startButton != null)
            startButton.interactable = false;

        Debug.Log("Combat Started: Attacker vs Defender");
    }

    public void ResetCombat()
    {
        combatInProgress = false;
        combatEnded = false;
        combatTimer = 0f;

        // HP 리셋
        if (attacker != null) attacker.ResetHP();
        if (defender != null) defender.ResetHP();

        if (resultText != null)
            resultText.text = "전투 시작 버튼을 눌러주세요";

        if (startButton != null)
            startButton.interactable = true;

        Debug.Log("Combat Reset");
    }

    void UpdateCombat()
    {
        combatTimer += Time.deltaTime;

        // 승패 조건 확인
        string result = "";

        // 사망 확인
        if (attacker != null && attacker.IsDead)
        {
            result = "방어형 승리! (공격형 사망)";
            EndCombat(result);
        }
        else if (defender != null && defender.IsDead)
        {
            result = "공격형 승리! (방어형 사망)";
            EndCombat(result);
        }
        // 시간 초과
        else if (combatTimer >= combatTimeLimit)
        {
            // HP 비교로 승부 결정
            float attackerHP = attacker != null ? attacker.HPPercentage : 0f;
            float defenderHP = defender != null ? defender.HPPercentage : 0f;

            if (attackerHP > defenderHP)
                result = "공격형 승리! (HP 우세)";
            else if (defenderHP > attackerHP)
                result = "방어형 승리! (HP 우세)";
            else
                result = "무승부!";

            EndCombat(result);
        }
    }

    void UpdateUI()
    {
        // HP 바 업데이트
        if (attacker != null)
        {
            if (attackerHPBar != null)
                attackerHPBar.value = attacker.HPPercentage;
            if (attackerHPText != null)
                attackerHPText.text = $"공격형: {attacker.CurrentHP:F0}/{attacker.MaxHP:F0}";
        }

        if (defender != null)
        {
            if (defenderHPBar != null)
                defenderHPBar.value = defender.HPPercentage;
            if (defenderHPText != null)
                defenderHPText.text = $"방어형: {defender.CurrentHP:F0}/{defender.MaxHP:F0}";
        }
    }

    void EndCombat(string result)
    {
        combatEnded = true;

        if (resultText != null)
            resultText.text = result;

        if (startButton != null)
            startButton.interactable = true;

        Debug.Log($"Combat Ended: {result}");
    }
}