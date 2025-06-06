using UnityEngine;
using BehaviorTree;

public class IsOutOfRange : ConditionNode
{
    private float maxRange;

    public IsOutOfRange(MonoBehaviour owner, Blackboard blackboard, float range = 5f)
        : base(owner, blackboard)
    {
        maxRange = range;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        Transform target = blackboard.GetValue<Transform>("target");

        if (controller == null || target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        float distance = Vector3.Distance(controller.transform.position, target.position);
        bool isOutOfRange = distance > maxRange;

        state = isOutOfRange ? NodeState.Success : NodeState.Failure;

        if (isOutOfRange)
        {
            Debug.Log($"IsOutOfRange: Target is out of range ({distance:F1}m > {maxRange}m)");
        }

        return state;
    }
}