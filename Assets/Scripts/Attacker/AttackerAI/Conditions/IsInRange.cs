using UnityEngine;
using BehaviorTree;

public class IsInRange : ConditionNode
{
    private float range;

    public IsInRange(MonoBehaviour owner, Blackboard blackboard, float range) : base(owner, blackboard)
    {
        this.range = range;
    }

    public override NodeState Evaluate()
    {
        Transform ownerTransform = owner.transform;
        Transform target = blackboard.GetValue<Transform>("target");

        if (target == null)
        {
            Debug.LogWarning("IsInRange: No target found in blackboard");
            return NodeState.Failure;
        }

        float distance = Vector3.Distance(ownerTransform.position, target.position);
        bool inRange = distance <= range;

        Debug.Log($"IsInRange: Distance={distance:F2}, Range={range}, InRange={inRange}");

        return inRange ? NodeState.Success : NodeState.Failure;
    }
}