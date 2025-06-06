using UnityEngine;
using BehaviorTree;

public class IsSafeToStrongAttack : ConditionNode
{
    public IsSafeToStrongAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        Transform target = blackboard.GetValue<Transform>("target");

        if (controller == null || target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        float distance = Vector3.Distance(controller.transform.position, target.position);

        // 조건 확인을 위한 변수들
        bool safeDistance = distance > 3f;
        bool opponentOnCooldown = false;
        bool opponentLowHP = false;

        // AttackerController 상대방인 경우
        AttackerController opponentAttacker = target.GetComponent<AttackerController>();
        if (opponentAttacker != null)
        {
            opponentOnCooldown = opponentAttacker.AttackCooldownRemaining > 1.5f;
            opponentLowHP = opponentAttacker.HPPercentage < 0.3f;
        }

        // DefenderController 상대방인 경우
        DefenderController opponentDefender = target.GetComponent<DefenderController>();
        if (opponentDefender != null)
        {
            opponentOnCooldown = opponentDefender.AttackCooldownRemaining > 1.5f;
            opponentLowHP = opponentDefender.HPPercentage < 0.3f;
        }

        // 조건 1: 거리 > 3m AND 상대 쿨타임 > 1.5초
        bool safeCondition = safeDistance && opponentOnCooldown;

        // 조건 2: 상대 HP < 30% AND 거리 <= 5m (피니싱 블로우)
        bool finishingBlow = opponentLowHP && distance <= 5f;

        bool isSafe = safeCondition || finishingBlow;
        state = isSafe ? NodeState.Success : NodeState.Failure;

        if (isSafe)
        {
            string reason = safeCondition ? "Safe distance + opponent cooldown" : "Finishing blow opportunity";
            Debug.Log($"IsSafeToStrongAttack: Safe to strong attack - {reason}");
        }

        return state;
    }
}