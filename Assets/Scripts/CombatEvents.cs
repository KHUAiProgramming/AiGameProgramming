using UnityEngine;

public static class CombatEvents
{
    // 공격 관련 이벤트
    public static System.Action<string, bool, float> OnAttackAttempt;  // 에이전트명, 성공여부, 데미지

    // 방어 관련 이벤트
    public static System.Action<string, bool> OnBlockAttempt;          // 에이전트명, 성공여부
    public static System.Action<string, string> OnStunCaused;         // 스턴시킨자, 스턴당한자

    // 회피 관련 이벤트
    public static System.Action<string, bool> OnDodgeAttempt;          // 에이전트명, 성공여부

    // 데미지 관련 이벤트
    public static System.Action<string, float> OnDamageTaken;          // 에이전트명, 데미지량

    // 발차기 관련 이벤트
    public static System.Action<string, bool> OnKickAttempt;           // 에이전트명, 방어뚫기성공여부

    // 이벤트 초기화
    public static void ClearAllEvents()
    {
        OnAttackAttempt = null;
        OnBlockAttempt = null;
        OnStunCaused = null;
        OnDodgeAttempt = null;
        OnDamageTaken = null;
        OnKickAttempt = null;
    }
}