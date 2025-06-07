using UnityEngine;
using BehaviorTree;

public class IsEnemyInRange : ConditionNode
{
    public IsEnemyInRange(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        Vector3 targetVector = enemy.position - self.transform.position;
        Debug.Log(targetVector);
        float distance = targetVector.magnitude;
        state = (distance < 1f) ? NodeState.Success : NodeState.Failure;
        return state;
    }
}