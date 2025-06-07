using UnityEngine;
using BehaviorTree;

public class IsEnemyInRange : ConditionNode
{
    public IsEnemyInRange(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        float distance = blackboard.GetValue<float>("enemyDistance");
        state = (distance < 5f) ? NodeState.Success : NodeState.Failure;
        return state;
    }
}