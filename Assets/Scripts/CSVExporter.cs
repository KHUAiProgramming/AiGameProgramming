using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class CSVExporter
{
    public static string SaveCombatResults(List<CombatResult> results, string fileName = "")
    {
        if (results == null || results.Count == 0)
        {
            Debug.LogWarning("저장할 전투 결과가 없습니다.");
            return "";
        }

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"CombatResults_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        }

        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder csv = new StringBuilder();

        // CSV 헤더 생성
        csv.AppendLine(CreateCSVHeader());

        // 각 전투 결과를 CSV 행으로 변환
        foreach (var result in results)
        {
            csv.AppendLine(CreateCSVRow(result));
        }

        try
        {
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            Debug.Log($"전투 결과가 저장되었습니다: {filePath}");
            return filePath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV 저장 중 오류 발생: {e.Message}");
            return "";
        }
    }

    public static string SaveSingleCombatResult(CombatResult result, string fileName = "")
    {
        if (result == null)
        {
            Debug.LogWarning("저장할 전투 결과가 없습니다.");
            return "";
        }

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"Combat_{result.combatNumber:D3}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        }

        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder csv = new StringBuilder();

        // CSV 헤더 생성
        csv.AppendLine(CreateCSVHeader());

        // 전투 결과를 CSV 행으로 변환
        csv.AppendLine(CreateCSVRow(result));

        try
        {
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            Debug.Log($"전투 {result.combatNumber} 결과가 저장되었습니다: {filePath}");
            return filePath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV 저장 중 오류 발생: {e.Message}");
            return "";
        }
    }

    private static string CreateCSVHeader()
    {
        return "전투번호,승자,전투시간,공격자최종HP,방어자최종HP," +
               // 공격자 기본 통계
               "공격자_공격시도,공격자_공격성공,공격자_공격실패,공격자_가한데미지,공격자_공격성공률," +
               "공격자_방어시도,공격자_방어성공,공격자_방어실패,공격자_방어성공률," +
               "공격자_회피시도,공격자_회피성공,공격자_회피성공률," +
               "공격자_받은데미지,공격자_발차기시도,공격자_발차기성공,공격자_방어로스턴,공격자_받은스턴," +
               // 공격자 분석 지표
               "공격자_방어적행동수,공격자_공격방어비율,공격자_효율점수," +
               // 방어자 기본 통계  
               "방어자_공격시도,방어자_공격성공,방어자_공격실패,방어자_가한데미지,방어자_공격성공률," +
               "방어자_방어시도,방어자_방어성공,방어자_방어실패,방어자_방어성공률," +
               "방어자_회피시도,방어자_회피성공,방어자_회피성공률," +
               "방어자_받은데미지,방어자_발차기시도,방어자_발차기성공,방어자_방어로스턴,방어자_받은스턴," +
               // 방어자 분석 지표
               "방어자_방어적행동수,방어자_공격방어비율,방어자_효율점수";
    }

    private static string CreateCSVRow(CombatResult result)
    {
        var a = result.attackerStats;  // 공격자 통계
        var d = result.defenderStats;  // 방어자 통계

        return $"{result.combatNumber},{result.winner},{result.duration:F2}," +
               $"{result.attackerFinalHP:F1},{result.defenderFinalHP:F1}," +

               // 공격자 기본 통계
               $"{a.attackAttempts},{a.attackHits},{a.attackMisses},{a.damageDealt:F1},{a.AttackSuccessRate:F1}," +
               $"{a.blockAttempts},{a.blockSuccesses},{a.blockFailures},{a.BlockSuccessRate:F1}," +
               $"{a.dodgeAttempts},{a.dodgeSuccesses},{a.DodgeSuccessRate:F1}," +
               $"{a.damageTaken:F1},{a.kickAttempts},{a.kickThroughDefense},{a.stunsCausedByBlock},{a.stunsTaken}," +
               // 공격자 분석 지표
               $"{a.TotalDefensiveActions},{a.AttackToDefenseRatio:F2},{a.EfficiencyScore:F2}," +

               // 방어자 기본 통계
               $"{d.attackAttempts},{d.attackHits},{d.attackMisses},{d.damageDealt:F1},{d.AttackSuccessRate:F1}," +
               $"{d.blockAttempts},{d.blockSuccesses},{d.blockFailures},{d.BlockSuccessRate:F1}," +
               $"{d.dodgeAttempts},{d.dodgeSuccesses},{d.DodgeSuccessRate:F1}," +
               $"{d.damageTaken:F1},{d.kickAttempts},{d.kickThroughDefense},{d.stunsCausedByBlock},{d.stunsTaken}," +
               // 방어자 분석 지표
               $"{d.TotalDefensiveActions},{d.AttackToDefenseRatio:F2},{d.EfficiencyScore:F2}";
    }
}