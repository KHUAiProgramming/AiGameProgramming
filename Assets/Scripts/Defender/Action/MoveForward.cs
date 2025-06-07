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

        Vector3 directionToEnemy = enemy.position - self.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
        self.transform.rotation = Quaternion.Slerp(self.transform.rotation, targetRotation, Time.deltaTime * 10f);
        Vector3 test = Vector3.right;
        //controller.Move(self.transform.forward);
        controller.Move(test);
        

        state = NodeState.Success;
        return state;
    }
}