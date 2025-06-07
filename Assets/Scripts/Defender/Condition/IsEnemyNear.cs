using UnityEngine;
using BehaviorTree;

public class IsEnemyNear : ConditionNode
{
    public IsEnemyNear(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        Vector3 targetVector = enemy.position - self.transform.position;
        float distance = targetVector.magnitude;
        state = (distance <= 3f) ? NodeState.Success : NodeState.Failure;
        if (state == NodeState.Success)
        {
            Debug.Log(distance);
        }
        return state;
    }
}