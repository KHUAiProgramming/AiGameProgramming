using UnityEngine;
using BehaviorTree;

public class IsEnemyFar : ConditionNode
{
    public IsEnemyFar(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        Vector3 targetVector = enemy.position - self.transform.position;
        Debug.Log(targetVector);
        float distance = targetVector.magnitude;
        state = (distance > 3f) ? NodeState.Success : NodeState.Failure;

        return state;
    }
}