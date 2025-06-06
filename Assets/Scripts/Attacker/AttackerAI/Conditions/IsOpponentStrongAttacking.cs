using UnityEngine;
using BehaviorTree;

public class IsOpponentStrongAttacking : ConditionNode
{
    public IsOpponentStrongAttacking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentStrongAttacking: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool isStrongAttacking = opponent.IsAttacking && opponent.CurrentAttackType == AttackerController.AttackType.Strong;
        Debug.Log($"IsOpponentStrongAttacking: {isStrongAttacking} (AttackType: {opponent.CurrentAttackType})");

        return isStrongAttacking ? NodeState.Success : NodeState.Failure;
    }
}