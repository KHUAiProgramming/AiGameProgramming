using UnityEngine;

[System.Serializable]
public class CombatResult
{
    public int combatNumber;
    public string winner;
    public float duration;
    public float attackerFinalHP;
    public float defenderFinalHP;
    
    // 통계 데이터
    public CombatStats attackerStats;
    public CombatStats defenderStats;
}