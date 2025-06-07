using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Attack : ActionNode
{
    public Attack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
    {
        
    }

    public override NodeState Evaluate()
    {
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        controller.WeakAttack();
        state = NodeState.Success;
        return state;
    }
}
