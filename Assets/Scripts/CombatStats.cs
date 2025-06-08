using UnityEngine;

[System.Serializable]
public class CombatStats
{
    [Header("공격 통계")]
    public int attackAttempts = 0;          // 공격 시도
    public int attackHits = 0;              // 공격 성공 (히트)
    public int attackMisses = 0;            // 공격 실패 (미스/회피당함)
    public float damageDealt = 0f;          // 가한 데미지

    [Header("방어 통계")]
    public int blockAttempts = 0;           // 방어 시도
    public int blockSuccesses = 0;          // 방어 성공 (데미지 차단)
    public int blockFailures = 0;           // 방어 실패 (데미지 받음)
    public int stunsCausedByBlock = 0;      // 방어로 상대 스턴시킨 횟수

    [Header("회피 통계")]
    public int dodgeAttempts = 0;           // 회피 시도
    public int dodgeSuccesses = 0;          // 회피 성공
    public float damageTaken = 0f;          // 받은 데미지

    [Header("특수 기술 통계")]
    public int kickAttempts = 0;            // 발차기 시도
    public int kickThroughDefense = 0;      // 발차기로 방어 뚫고 공격
    public int stunsTaken = 0;              // 받은 스턴 횟수

    [Header("전투 정보")]
    public float combatDuration = 0f;       // 전투 지속 시간
    public float finalHP = 0f;              // 최종 HP

    // 통계 초기화
    public void Reset()
    {
        attackAttempts = 0;
        attackHits = 0;
        attackMisses = 0;
        damageDealt = 0f;

        blockAttempts = 0;
        blockSuccesses = 0;
        blockFailures = 0;
        stunsCausedByBlock = 0;

        dodgeAttempts = 0;
        dodgeSuccesses = 0;
        damageTaken = 0f;

        kickAttempts = 0;
        kickThroughDefense = 0;
        stunsTaken = 0;

        combatDuration = 0f;
        finalHP = 0f;
    }

    // 핵심 분석 지표들
    public float AttackSuccessRate => attackAttempts > 0 ? (float)attackHits / attackAttempts * 100f : 0f;
    public float BlockSuccessRate => blockAttempts > 0 ? (float)blockSuccesses / blockAttempts * 100f : 0f;
    public float DodgeSuccessRate => dodgeAttempts > 0 ? (float)dodgeSuccesses / dodgeAttempts * 100f : 0f;
    public float KickSuccessRate => kickAttempts > 0 ? (float)kickThroughDefense / kickAttempts * 100f : 0f;
    public float AverageDamagePerHit => attackHits > 0 ? damageDealt / attackHits : 0f;

    // 에이전트 특성 분석을 위한 추가 지표
    public int TotalDefensiveActions => blockAttempts + dodgeAttempts;  // 총 방어적 행동
    public float AttackToDefenseRatio => TotalDefensiveActions > 0 ? (float)attackAttempts / TotalDefensiveActions : float.MaxValue;
    public float EfficiencyScore => attackAttempts > 0 ? (damageDealt / attackAttempts) : 0f; // 공격당 효율
}