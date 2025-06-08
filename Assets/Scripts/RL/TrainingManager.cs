using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Header("Training Mode Selection")]
    [SerializeField] private TrainingMode currentMode = TrainingMode.AttackerRL_vs_BTDefender;

    [Header("Existing Agents (씬에 이미 있는 에이전트들)")]
    [SerializeField] private GameObject existingAttacker;
    [SerializeField] private GameObject existingDefender;

    [Header("Agent Prefabs (새로 생성할 경우만 사용)")]
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

    [Header("Use Existing Agents (체크하면 씬의 기존 에이전트 사용)")]
    [SerializeField] private bool useExistingAgents = true;

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
        if (useExistingAgents)
        {
            FindExistingAgents();
        }
        
        SetupTrainingEnvironment();
    }

    void FindExistingAgents()
    {
        // 씬에서 기존 에이전트들을 찾기
        if (existingAttacker == null)
        {
            // AttackerController가 있는 게임오브젝트 찾기
            AttackerController[] attackers = FindObjectsOfType<AttackerController>();
            if (attackers.Length > 0)
            {
                existingAttacker = attackers[0].gameObject;
                Debug.Log($"Found existing attacker: {existingAttacker.name}");
            }
        }

        if (existingDefender == null)
        {
            // DefenderController가 있는 게임오브젝트 찾기
            DefenderController[] defenders = FindObjectsOfType<DefenderController>();
            if (defenders.Length > 0)
            {
                existingDefender = defenders[0].gameObject;
                Debug.Log($"Found existing defender: {existingDefender.name}");
            }
        }

        // 스폰 포인트를 기존 에이전트 위치로 설정
        if (existingAttacker != null && attackerSpawnPoint == null)
        {
            attackerSpawnPoint = existingAttacker.transform;
        }

        if (existingDefender != null && defenderSpawnPoint == null)
        {
            defenderSpawnPoint = existingDefender.transform;
        }
    }

    void SetupTrainingEnvironment()
    {
        // 기존에 생성된 임시 에이전트들만 제거 (원본은 유지)
        if (!useExistingAgents)
        {
            if (currentAttacker != null && currentAttacker != existingAttacker) 
                DestroyImmediate(currentAttacker);
            if (currentDefender != null && currentDefender != existingDefender) 
                DestroyImmediate(currentDefender);
        }

        // 스폰 포인트 기본 설정
        SetupDefaultSpawnPoints();

        // 모드에 따라 에이전트 설정
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

    void SetupDefaultSpawnPoints()
    {
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
    }

    void SetupAttackerRLTraining()
    {
        // 공격형: RL 에이전트 설정
        if (useExistingAgents && existingAttacker != null)
        {
            currentAttacker = existingAttacker;
            
            // 기존 BT 제거하고 RL 에이전트 추가
            AttackerBT existingBT = currentAttacker.GetComponent<AttackerBT>();
            if (existingBT != null)
            {
                existingBT.enabled = false; // BT 비활성화
            }

            AttackerRLAgent rlAgent = currentAttacker.GetComponent<AttackerRLAgent>();
            if (rlAgent == null)
            {
                rlAgent = currentAttacker.AddComponent<AttackerRLAgent>();
            }
            rlAgent.enabled = true;

            currentAttacker.name = "AttackerRL";
        }
        else if (attackerRLPrefab != null)
        {
            currentAttacker = Instantiate(attackerRLPrefab, attackerSpawnPoint.position, attackerSpawnPoint.rotation);
            currentAttacker.name = "AttackerRL";
        }

        // 방어형: BT 에이전트 설정
        if (useExistingAgents && existingDefender != null)
        {
            currentDefender = existingDefender;
            
            // RL 에이전트가 있다면 비활성화하고 BT 활성화
            DefenderRLAgent existingRL = currentDefender.GetComponent<DefenderRLAgent>();
            if (existingRL != null)
            {
                existingRL.enabled = false;
            }

            DefenderBT btAgent = currentDefender.GetComponent<DefenderBT>();
            if (btAgent == null)
            {
                btAgent = currentDefender.AddComponent<DefenderBT>();
            }
            btAgent.enabled = true;

            currentDefender.name = "DefenderBT";
        }
        else if (defenderRLPrefab != null) // 방어형 BT 프리팹이 없다면 RL 프리팹 사용하고 BT 추가
        {
            currentDefender = Instantiate(defenderRLPrefab, defenderSpawnPoint.position, defenderSpawnPoint.rotation);
            currentDefender.name = "DefenderBT";
            
            // RL 에이전트 비활성화하고 BT 추가
            DefenderRLAgent rlAgent = currentDefender.GetComponent<DefenderRLAgent>();
            if (rlAgent != null) rlAgent.enabled = false;
            
            DefenderBT btAgent = currentDefender.AddComponent<DefenderBT>();
            btAgent.enabled = true;
        }

        SetupAgentReferences();
    }

    void SetupDefenderRLTraining()
    {
        // 공격형: BT 에이전트 설정
        if (useExistingAgents && existingAttacker != null)
        {
            currentAttacker = existingAttacker;
            
            // RL 에이전트 비활성화하고 BT 활성화
            AttackerRLAgent existingRL = currentAttacker.GetComponent<AttackerRLAgent>();
            if (existingRL != null)
            {
                existingRL.enabled = false;
            }

            AttackerBT btAgent = currentAttacker.GetComponent<AttackerBT>();
            if (btAgent == null)
            {
                btAgent = currentAttacker.AddComponent<AttackerBT>();
            }
            btAgent.enabled = true;

            currentAttacker.name = "AttackerBT";
        }
        else if (attackerBTPrefab != null)
        {
            currentAttacker = Instantiate(attackerBTPrefab, attackerSpawnPoint.position, attackerSpawnPoint.rotation);
            currentAttacker.name = "AttackerBT";
        }

        // 방어형: RL 에이전트 설정
        if (useExistingAgents && existingDefender != null)
        {
            currentDefender = existingDefender;
            
            // BT 비활성화하고 RL 활성화
            DefenderBT existingBT = currentDefender.GetComponent<DefenderBT>();
            if (existingBT != null)
            {
                existingBT.enabled = false;
            }

            DefenderRLAgent rlAgent = currentDefender.GetComponent<DefenderRLAgent>();
            if (rlAgent == null)
            {
                rlAgent = currentDefender.AddComponent<DefenderRLAgent>();
            }
            rlAgent.enabled = true;

            currentDefender.name = "DefenderRL";
        }
        else if (defenderRLPrefab != null)
        {
            currentDefender = Instantiate(defenderRLPrefab, defenderSpawnPoint.position, defenderSpawnPoint.rotation);
            currentDefender.name = "DefenderRL";

            // DefenderRLAgent 설정
            DefenderRLAgent rlAgent = currentDefender.GetComponent<DefenderRLAgent>();
            if (rlAgent == null)
                rlAgent = currentDefender.AddComponent<DefenderRLAgent>();
        }

        SetupAgentReferences();
    }

    void SetupMultiAgentTraining()
    {
        // 공격형: RL 에이전트 설정
        if (useExistingAgents && existingAttacker != null)
        {
            currentAttacker = existingAttacker;
            
            AttackerBT existingBT = currentAttacker.GetComponent<AttackerBT>();
            if (existingBT != null) existingBT.enabled = false;

            AttackerRLAgent rlAgent = currentAttacker.GetComponent<AttackerRLAgent>();
            if (rlAgent == null)
                rlAgent = currentAttacker.AddComponent<AttackerRLAgent>();
            rlAgent.enabled = true;

            currentAttacker.name = "AttackerRL";
        }
        else if (attackerRLPrefab != null)
        {
            currentAttacker = Instantiate(attackerRLPrefab, attackerSpawnPoint.position, attackerSpawnPoint.rotation);
            currentAttacker.name = "AttackerRL";

            AttackerRLAgent attackerRL = currentAttacker.GetComponent<AttackerRLAgent>();
            if (attackerRL == null)
                attackerRL = currentAttacker.AddComponent<AttackerRLAgent>();
        }

        // 방어형: RL 에이전트 설정
        if (useExistingAgents && existingDefender != null)
        {
            currentDefender = existingDefender;
            
            DefenderBT existingBT = currentDefender.GetComponent<DefenderBT>();
            if (existingBT != null) existingBT.enabled = false;

            DefenderRLAgent rlAgent = currentDefender.GetComponent<DefenderRLAgent>();
            if (rlAgent == null)
                rlAgent = currentDefender.AddComponent<DefenderRLAgent>();
            rlAgent.enabled = true;

            currentDefender.name = "DefenderRL";
        }
        else if (defenderRLPrefab != null)
        {
            currentDefender = Instantiate(defenderRLPrefab, defenderSpawnPoint.position, defenderSpawnPoint.rotation);
            currentDefender.name = "DefenderRL";

            DefenderRLAgent defenderRL = currentDefender.GetComponent<DefenderRLAgent>();
            if (defenderRL == null)
                defenderRL = currentDefender.AddComponent<DefenderRLAgent>();
        }

        SetupAgentReferences();
    }

    void SetupAgentReferences()
    {
        if (currentAttacker == null || currentDefender == null) return;

        // AttackerRLAgent 참조 설정
        AttackerRLAgent attackerRL = currentAttacker.GetComponent<AttackerRLAgent>();
        if (attackerRL != null && attackerRL.enabled)
        {
            DefenderController defenderController = currentDefender.GetComponent<DefenderController>();
            if (defenderController != null)
            {
                // SerializeField로 변경된 필드들을 직접 설정
                System.Reflection.FieldInfo btDefenderField = typeof(AttackerRLAgent).GetField("btDefender",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                btDefenderField?.SetValue(attackerRL, defenderController);

                System.Reflection.FieldInfo defenderTransformField = typeof(AttackerRLAgent).GetField("defenderTransform",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                defenderTransformField?.SetValue(attackerRL, currentDefender.transform);
            }
        }

        // DefenderRLAgent 참조 설정
        DefenderRLAgent defenderRL = currentDefender.GetComponent<DefenderRLAgent>();
        if (defenderRL != null && defenderRL.enabled)
        {
            AttackerController attackerController = currentAttacker.GetComponent<AttackerController>();
            if (attackerController != null)
            {
                System.Reflection.FieldInfo btAttackerField = typeof(DefenderRLAgent).GetField("btAttacker",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                btAttackerField?.SetValue(defenderRL, attackerController);

                System.Reflection.FieldInfo attackerTransformField = typeof(DefenderRLAgent).GetField("attackerTransform",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                attackerTransformField?.SetValue(defenderRL, currentAttacker.transform);
            }
        }

        // BT 에이전트들의 타겟 설정
        AttackerBT attackerBT = currentAttacker.GetComponent<AttackerBT>();
        if (attackerBT != null && attackerBT.enabled)
        {
            attackerBT.SetTarget(currentDefender.transform);
        }

        DefenderBT defenderBT = currentDefender.GetComponent<DefenderBT>();
        if (defenderBT != null && defenderBT.enabled)
        {
            defenderBT.SetTarget(currentAttacker.transform);
        }

        Debug.Log("Agent references setup complete");
    }

    [ContextMenu("Switch Training Mode")]
    public void SwitchTrainingMode()
    {
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

    [ContextMenu("Find Existing Agents")]
    public void FindExistingAgentsManually()
    {
        FindExistingAgents();
        Debug.Log($"Found - Attacker: {existingAttacker?.name}, Defender: {existingDefender?.name}");
    }

    void OnValidate()
    {
        if (Application.isPlaying && useExistingAgents)
        {
            FindExistingAgents();
        }
    }
}