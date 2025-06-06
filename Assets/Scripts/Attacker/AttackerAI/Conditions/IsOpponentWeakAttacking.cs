using UnityEngine;
using BehaviorTree;

public class IsOpponentWeakAttacking : ConditionNode
{
    public IsOpponentWeakAttacking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");
        
        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentWeakAttacking: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool isWeakAttacking = opponent.IsAttacking && opponent.CurrentAttackType == AttackerController.AttackType.Weak;
        Debug.Log($"IsOpponentWeakAttacking: {isWeakAttacking} (AttackType: {opponent.CurrentAttackType})");
        
        return isWeakAttacking ? NodeState.Success : NodeState.Failure;
    }
}