using UnityEngine;
using BehaviorTree;

public class Aim : ActionNode
{
    public Aim(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        Vector3 targetVector = enemy.position - self.transform.position;
        Vector3 direction = targetVector.normalized;

        //Debug.Log("Aim");
        self.transform.LookAt(enemy);

        state = NodeState.Success;
        return state;
    }
}