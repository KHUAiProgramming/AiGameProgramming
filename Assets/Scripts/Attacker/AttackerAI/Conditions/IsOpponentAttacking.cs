using UnityEngine;
using BehaviorTree;

public class IsOpponentAttacking : ConditionNode
{
    public IsOpponentAttacking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentAttacking: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool isOpponentAttacking = opponent.IsAttacking;
        Debug.Log($"IsOpponentAttacking: {isOpponentAttacking}");

        return isOpponentAttacking ? NodeState.Success : NodeState.Failure;
    }
}