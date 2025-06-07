using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Shield : ActionNode
{
    Defender defender;

    public Shield(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
    {
        defender = blackboard.GetValue<Defender>("defender");
    }

    public override NodeState Evaluate()
    {
        DefenderController controller = blackboard.GetValue<DefenderController>("controller");

        controller.Block();
        state = NodeState.Success;
        return state;
    }
}