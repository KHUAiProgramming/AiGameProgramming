using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatSimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int totalCombats = 100;
    public float combatTimeLimit = 120f; // 전투 시간 제한 (무한 루프 방지)
    public float resetDelay = 2f; // 전투 종료 후 대기 시간

    [Header("Agent References")]
    public AttackerController attacker;
    public DefenderController defender;

    [Header("Spawn Points")]
    public Transform attackerSpawnPoint;
    public Transform defenderSpawnPoint;

    [Header("Combat Data")]
    [SerializeField] private int currentCombatNumber = 0;
    [SerializeField] private int attackerWins = 0;
    [SerializeField] private int defenderWins = 0;
    [SerializeField] private int draws = 0;

    // Combat state tracking
    private bool combatInProgress = false;
    private float combatStartTime;
    private string lastWinner = "";

    // Combat results storage
    private List<CombatResult> combatResults = new List<CombatResult>();

    void Start()
    {
        // 컴포넌트 검증
        ValidateComponents();

        // 첫 번째 전투 시작
        StartNextCombat();
    }

    void Update()
    {
        if (combatInProgress)
        {
            CheckCombatEnd();
        }
    }

    private void ValidateComponents()
    {
        if (attacker == null)
        {
            GameObject attackerObj = GameObject.Find("Maria WProp J J Ong");
            if (attackerObj != null)
                attacker = attackerObj.GetComponent<AttackerController>();
        }

        if (defender == null)
        {
            GameObject defenderObj = GameObject.Find("Defender");
            if (defenderObj != null)
                defender = defenderObj.GetComponent<DefenderController>();
        }

        if (attackerSpawnPoint == null)
        {
            GameObject spawnObj = GameObject.Find("AttackerSpawn");
            if (spawnObj != null)
                attackerSpawnPoint = spawnObj.transform;
        }

        if (defenderSpawnPoint == null)
        {
            GameObject spawnObj = GameObject.Find("DefenderSpawn");
            if (spawnObj != null)
                defenderSpawnPoint = spawnObj.transform;
        }

        // 기본 스폰 포인트 생성 (없을 경우)
        if (attackerSpawnPoint == null)
        {
            GameObject newSpawn = new GameObject("AttackerSpawn");
            newSpawn.transform.position = new Vector3(-3f, 0f, 0f);
            attackerSpawnPoint = newSpawn.transform;
        }

        if (defenderSpawnPoint == null)
        {
            GameObject newSpawn = new GameObject("DefenderSpawn");
            newSpawn.transform.position = new Vector3(3f, 0f, 0f);
            defenderSpawnPoint = newSpawn.transform;
        }

        Debug.Log($"Combat Manager initialized - Attacker: {attacker?.name}, Defender: {defender?.name}");
    }

    private void StartNextCombat()
    {
        if (currentCombatNumber >= totalCombats)
        {
            EndSimulation();
            return;
        }

        currentCombatNumber++;
        Debug.Log($"=== 전투 {currentCombatNumber}/{totalCombats} 시작 ===");

        // 에이전트 초기화 및 배치
        ResetAgents();

        // 전투 상태 초기화
        combatInProgress = true;
        combatStartTime = Time.time;
        lastWinner = "";
    }

    private void ResetAgents()
    {
        // 위치 초기화
        if (attacker != null && attackerSpawnPoint != null)
        {
            attacker.transform.position = attackerSpawnPoint.position;
            attacker.transform.rotation = attackerSpawnPoint.rotation;
        }

        if (defender != null && defenderSpawnPoint != null)
        {
            defender.transform.position = defenderSpawnPoint.position;
            defender.transform.rotation = defenderSpawnPoint.rotation;
        }

        // HP 및 상태 초기화
        if (attacker != null)
        {
            attacker.ResetHP();
            // 추가 상태 초기화가 필요하면 여기에
        }

        if (defender != null)
        {
            defender.ResetHP();
            // 추가 상태 초기화가 필요하면 여기에
        }

        Debug.Log("에이전트 위치 및 상태 초기화 완료");
    }

    private void CheckCombatEnd()
    {
        // 시간 제한 체크
        if (Time.time - combatStartTime >= combatTimeLimit)
        {
            EndCombat("Draw - Time Limit");
            return;
        }

        // 에이전트 상태 체크
        bool attackerDead = attacker?.IsDead ?? false;
        bool defenderDead = defender?.IsDead ?? false;

        if (attackerDead && defenderDead)
        {
            EndCombat("Draw - Both Dead");
        }
        else if (attackerDead)
        {
            EndCombat("Defender");
        }
        else if (defenderDead)
        {
            EndCombat("Attacker");
        }
    }

    private void EndCombat(string winner)
    {
        combatInProgress = false;
        lastWinner = winner;
        float combatDuration = Time.time - combatStartTime;

        // 승리 카운트 업데이트
        switch (winner)
        {
            case "Attacker":
                attackerWins++;
                break;
            case "Defender":
                defenderWins++;
                break;
            default:
                draws++;
                break;
        }

        Debug.Log($"전투 {currentCombatNumber} 종료: 승자 - {winner}, 지속시간: {combatDuration:F2}초");
        Debug.Log($"현재 스코어 - 공격형: {attackerWins}, 방어형: {defenderWins}, 무승부: {draws}");

        // 전투 결과 저장
        CombatResult result = new CombatResult
        {
            combatNumber = currentCombatNumber,
            winner = winner,
            duration = combatDuration,
            attackerFinalHP = attacker?.CurrentHP ?? 0f,
            defenderFinalHP = defender?.CurrentHP ?? 0f
        };
        combatResults.Add(result);

        // 다음 전투 준비
        StartCoroutine(PrepareNextCombat());
    }

    private IEnumerator PrepareNextCombat()
    {
        // 대기 시간
        yield return new WaitForSeconds(resetDelay);

        // 다음 전투 시작
        StartNextCombat();
    }

    private void EndSimulation()
    {
        Debug.Log("=== 전투 시뮬레이션 완료 ===");
        Debug.Log($"총 전투 횟수: {totalCombats}");
        Debug.Log($"공격형 승리: {attackerWins} ({(float)attackerWins / totalCombats * 100:F1}%)");
        Debug.Log($"방어형 승리: {defenderWins} ({(float)defenderWins / totalCombats * 100:F1}%)");
        Debug.Log($"무승부: {draws} ({(float)draws / totalCombats * 100:F1}%)");

        // CSV 파일로 저장 (추후 구현)
        SaveResultsToCSV();
    }

    private void SaveResultsToCSV()
    {
        // CSV 저장 로직은 다음 단계에서 구현
        Debug.Log("CSV 저장 기능은 다음 단계에서 구현됩니다.");
    }

    // 에디터용 디버그 메서드
    [ContextMenu("Start Next Combat")]
    public void StartNextCombatDebug()
    {
        if (!combatInProgress)
            StartNextCombat();
    }

    [ContextMenu("Reset Simulation")]
    public void ResetSimulation()
    {
        currentCombatNumber = 0;
        attackerWins = 0;
        defenderWins = 0;
        draws = 0;
        combatResults.Clear();
        combatInProgress = false;

        Debug.Log("시뮬레이션 리셋 완료");
    }
}

[System.Serializable]
public class CombatResult
{
    public int combatNumber;
    public string winner;
    public float duration;
    public float attackerFinalHP;
    public float defenderFinalHP;

    // 추후 통계 데이터 추가
    // public CombatStats attackerStats;
    // public CombatStats defenderStats;
}