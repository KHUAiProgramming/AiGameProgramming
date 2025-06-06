using UnityEngine;
using BehaviorTree;

public class IsOpponentBlocking : ConditionNode
{
    public IsOpponentBlocking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentBlocking: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool isBlocking = opponent.IsBlocking;
        Debug.Log($"IsOpponentBlocking: {isBlocking}");

        return isBlocking ? NodeState.Success : NodeState.Failure;
    }
}