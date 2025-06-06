using UnityEngine;
using BehaviorTree;

public class IsOpponentThreatening : ConditionNode
{
    private float threateningThreshold = 1f; // 1초 이하면 위협적

    public IsOpponentThreatening(MonoBehaviour owner, Blackboard blackboard, float threshold = 1f)
        : base(owner, blackboard)
    {
        threateningThreshold = threshold;
    }

    public override NodeState Evaluate()
    {
        Transform target = blackboard.GetValue<Transform>("target");

        if (target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        bool isThreatening = false;

        // AttackerController인 경우
        AttackerController opponentAttacker = target.GetComponent<AttackerController>();
        if (opponentAttacker != null)
        {
            isThreatening = opponentAttacker.AttackCooldownRemaining <= threateningThreshold;
        }

        // DefenderController인 경우
        DefenderController opponentDefender = target.GetComponent<DefenderController>();
        if (opponentDefender != null)
        {
            isThreatening = opponentDefender.AttackCooldownRemaining <= threateningThreshold;
        }

        state = isThreatening ? NodeState.Success : NodeState.Failure;

        if (isThreatening)
        {
            Debug.Log($"IsOpponentThreatening: Opponent is threatening (cooldown <= {threateningThreshold}s)");
        }

        return state;
    }
}