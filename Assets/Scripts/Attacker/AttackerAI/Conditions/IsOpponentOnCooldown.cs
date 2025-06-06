using UnityEngine;
using BehaviorTree;

public class IsOpponentOnCooldown : ConditionNode
{
    public IsOpponentOnCooldown(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentOnCooldown: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool onCooldown = opponent.AttackCooldownRemaining > 0f;
        Debug.Log($"IsOpponentOnCooldown: {onCooldown} (Remaining: {opponent.AttackCooldownRemaining:F2}s)");

        return onCooldown ? NodeState.Success : NodeState.Failure;
    }
}