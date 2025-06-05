using UnityEngine;
using BehaviorTree;
using System;
public class StrongAttack : ActionNode
{
    public StrongAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
    {
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            state = NodeState.Failure;
            return state;
        }

        if (controller.CanAttack())
        {
            controller.Attack();
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Running;
        }

        return state;
    }

    public override void Reset()
    {
        base.Reset();
    }
}