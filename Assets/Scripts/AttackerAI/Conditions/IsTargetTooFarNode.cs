using UnityEngine;
using BehaviorTree;

public class IsTargetTooFarNode : ConditionNode
{
    private float maxDistance;

    public IsTargetTooFarNode(MonoBehaviour owner, Blackboard blackboard, float maxDistance = 5f)
        : base(owner, blackboard)
    {
        this.maxDistance = maxDistance;
    }

    public override NodeState Evaluate()
    {
        Transform target = blackboard.GetValue<Transform>("target");
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");

        if (self.transform == null || target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        float distance = Vector3.Distance(self.transform.position, target.position);

        if (distance > maxDistance)
        {
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Failure;
        }

        return state;
    }
}
