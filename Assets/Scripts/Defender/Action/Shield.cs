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
        //Debug.Log("Raising shield!");
        if (defender != null)
        {
            defender.SendMessage("Shield");
        }

        state = NodeState.Success;
        return state;
    }
}