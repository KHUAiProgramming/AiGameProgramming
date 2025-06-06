using UnityEngine;
using BehaviorTree;

public class IsOpponentDodging : ConditionNode
{
    public IsOpponentDodging(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (opponent == null)
        {
            Debug.LogWarning("IsOpponentDodging: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        bool isDodging = opponent.IsDodging;
        Debug.Log($"IsOpponentDodging: {isDodging}");

        return isDodging ? NodeState.Success : NodeState.Failure;
    }
}