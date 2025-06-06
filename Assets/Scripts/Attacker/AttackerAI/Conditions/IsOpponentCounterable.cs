using UnityEngine;
using BehaviorTree;

public class IsOpponentCounterable : ConditionNode
{
    public IsOpponentCounterable(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");
        
        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentCounterable: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool isCounterable = opponent.CanBeCountered;
        Debug.Log($"IsOpponentCounterable: {isCounterable}");
        
        return isCounterable ? NodeState.Success : NodeState.Failure;
    }
}