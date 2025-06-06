using UnityEngine;
using BehaviorTree;

public class HasTarget : ConditionNode
{
    public HasTarget(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        Transform target = blackboard.GetValue<Transform>("target");

        bool hasTarget = target != null;
        Debug.Log($"HasTarget: {hasTarget}");

        return hasTarget ? NodeState.Success : NodeState.Failure;
    }
}