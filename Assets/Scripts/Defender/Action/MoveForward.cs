using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class MoveForward : ActionNode
{
    public MoveForward(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        Debug.Log("Moving forward from enemy...");
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        Vector3 targetVector = enemy.position - self.transform.position;
        Vector3 direction = targetVector.normalized;
        
        self.transform.LookAt(enemy);
        controller.Move(self.transform.forward);
        

        state = NodeState.Success;
        return state;
    }
}