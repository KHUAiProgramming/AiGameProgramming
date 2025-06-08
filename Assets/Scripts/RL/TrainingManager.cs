using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Header("Training Mode Selection")]
    [SerializeField] private TrainingMode currentMode = TrainingMode.AttackerRL_vs_BTDefender;

    [Header("Agent References")]
    [SerializeField] private GameObject attackerRLPrefab;
    [SerializeField] private GameObject defenderRLPrefab;
    [SerializeField] private GameObject attackerBTPrefab;
    [SerializeField] private GameObject defenderBTPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform attackerSpawnPoint;
    [SerializeField] private Transform defenderSpawnPoint;

    [Header("Training Settings")]
    [SerializeField] private int maxEpisodes = 1000;
    [SerializeField] private float episodeTimeout = 30f;

    private GameObject currentAttacker;
    private GameObject currentDefender;
    private int currentEpisode = 0;

    public enum TrainingMode
    {
        AttackerRL_vs_BTDefender,  // RL 공격형 vs BT 방어형
        DefenderRL_vs_BTAttacker,  // RL 방어형 vs BT 공격형
        Both_RL                    // RL 공격형 vs RL 방어형 (상호 학습)
    }

    void Start()
    {
        SetupTrainingEnvironment();
    }

    void SetupTrainingEnvironment()
    {
        // 기존 에이전트들 제거
        if (currentAttacker != null) DestroyImmediate(currentAttacker);
        if (currentDefender != null) DestroyImmediate(currentDefender);

        // 스폰 포인트 설정
        if (attackerSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("AttackerSpawn");
            spawnObj.transform.position = new Vector3(-3f, 0f, 0f);
            attackerSpawnPoint = spawnObj.transform;
        }

        if (defenderSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("DefenderSpawn");
            spawnObj.transform.position = new Vector3(3f, 0f, 0f);
            defenderSpawnPoint = spawnObj.transform;
        }

        // 모드에 따라 에이전트 생성
        switch (currentMode)
        {
            case TrainingMode.AttackerRL_vs_BTDefender:
                SetupAttackerRLTraining();
                break;

            case TrainingMode.DefenderRL_vs_BTAttacker:
                SetupDefenderRLTraining();
                break;

            case TrainingMode.Both_RL:
                SetupMultiAgentTraining();
                break;
        }

        Debug.Log($"Training Environment Setup Complete - Mode: {currentMode}");
    }

    void SetupAttackerRLTraining()
    {
        // RL 공격형 생성
        if (attackerRLPrefab != null)
        {
            currentAttacker = Instantiate(attackerRLPrefab, attackerSpawnPoint.position, attackerSpawnPoint.rotation);
            currentAttacker.name = "AttackerRL";

            // AttackerRLAgent 설정
            AttackerRLAgent rlAgent = currentAttacker.GetComponent<AttackerRLAgent>();
            if (rlAgent == null)
                rlAgent = currentAttacker.AddComponent<AttackerRLAgent>();
        }

        // BT 방어형 생성
        if (defenderBTPrefab != null)
        {
            currentDefender = Instantiate(defenderBTPrefab, defenderSpawnPoint.position, defenderSpawnPoint.rotation);
            currentDefender.name = "DefenderBT";
        }

        // 상호 참조 설정
        SetupAgentReferences();
    }

    void SetupDefenderRLTraining()
    {
        // BT 공격형 생성
        if (attackerBTPrefab != null)
        {
            currentAttacker = Instantiate(attackerBTPrefab, attackerSpawnPoint.position, attackerSpawnPoint.rotation);
            currentAttacker.name = "AttackerBT";
        }

        // RL 방어형 생성
        if (defenderRLPrefab != null)
        {
            currentDefender = Instantiate(defenderRLPrefab, defenderSpawnPoint.position, defenderSpawnPoint.rotation);
            currentDefender.name = "DefenderRL";

            // DefenderRLAgent 설정
            DefenderRLAgent rlAgent = currentDefender.GetComponent<DefenderRLAgent>();
            if (rlAgent == null)
                rlAgent = currentDefender.AddComponent<DefenderRLAgent>();
        }

        // 상호 참조 설정
        SetupAgentReferences();
    }

    void SetupMultiAgentTraining()
    {
        // RL 공격형 생성
        if (attackerRLPrefab != null)
        {
            currentAttacker = Instantiate(attackerRLPrefab, attackerSpawnPoint.position, attackerSpawnPoint.rotation);
            currentAttacker.name = "AttackerRL";

            AttackerRLAgent attackerRL = currentAttacker.GetComponent<AttackerRLAgent>();
            if (attackerRL == null)
                attackerRL = currentAttacker.AddComponent<AttackerRLAgent>();
        }

        // RL 방어형 생성
        if (defenderRLPrefab != null)
        {
            currentDefender = Instantiate(defenderRLPrefab, defenderSpawnPoint.position, defenderSpawnPoint.rotation);
            currentDefender.name = "DefenderRL";

            DefenderRLAgent defenderRL = currentDefender.GetComponent<DefenderRLAgent>();
            if (defenderRL == null)
                defenderRL = currentDefender.AddComponent<DefenderRLAgent>();
        }

        // 상호 참조 설정
        SetupAgentReferences();
    }

    void SetupAgentReferences()
    {
        if (currentAttacker == null || currentDefender == null) return;

        // AttackerRLAgent 참조 설정
        AttackerRLAgent attackerRL = currentAttacker.GetComponent<AttackerRLAgent>();
        if (attackerRL != null)
        {
            DefenderController btDefender = currentDefender.GetComponent<DefenderController>();
            if (btDefender != null)
            {
                // 리플렉션을 사용하여 private 필드 설정
                var field = typeof(AttackerRLAgent).GetField("btDefender",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(attackerRL, btDefender);

                var transformField = typeof(AttackerRLAgent).GetField("defenderTransform",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                transformField?.SetValue(attackerRL, currentDefender.transform);
            }
        }

        // DefenderRLAgent 참조 설정
        DefenderRLAgent defenderRL = currentDefender.GetComponent<DefenderRLAgent>();
        if (defenderRL != null)
        {
            AttackerController btAttacker = currentAttacker.GetComponent<AttackerController>();
            if (btAttacker != null)
            {
                // 리플렉션을 사용하여 private 필드 설정
                var field = typeof(DefenderRLAgent).GetField("btAttacker",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(defenderRL, btAttacker);

                var transformField = typeof(DefenderRLAgent).GetField("attackerTransform",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                transformField?.SetValue(defenderRL, currentAttacker.transform);
            }
        }

        // BT 에이전트들의 타겟 설정
        AttackerBT attackerBT = currentAttacker.GetComponent<AttackerBT>();
        if (attackerBT != null)
        {
            attackerBT.SetTarget(currentDefender.transform);
        }

        DefenderBT defenderBT = currentDefender.GetComponent<DefenderBT>();
        if (defenderBT != null)
        {
            defenderBT.SetTarget(currentAttacker.transform);
        }

        Debug.Log("Agent references setup complete");
    }

    [ContextMenu("Switch Training Mode")]
    public void SwitchTrainingMode()
    {
        // 다음 모드로 전환
        int currentIndex = (int)currentMode;
        currentIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(TrainingMode)).Length;
        currentMode = (TrainingMode)currentIndex;

        SetupTrainingEnvironment();
    }

    [ContextMenu("Reset Training Environment")]
    public void ResetTrainingEnvironment()
    {
        SetupTrainingEnvironment();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetupTrainingEnvironment();
        }
    }
}