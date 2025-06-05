using UnityEngine;
using BehaviorTree;

public class IsInStrongAttackRange : ConditionNode
{
    public IsInStrongAttackRange(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");

        float distance = Vector3.Distance(self.transform.position, enemy.position);

        if (distance >= 4f && distance <= 5f)
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