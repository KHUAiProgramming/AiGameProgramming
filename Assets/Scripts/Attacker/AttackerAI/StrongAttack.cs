using UnityEngine;
using BehaviorTree;
using System;

public class StrongAttack : ActionNode
{
    private bool attackStarted = false;

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

        if (!attackStarted)
        {
            if (controller.CanAttack())
            {
                controller.Attack();
                attackStarted = true;
                state = NodeState.Running;
            }
            else
            {
                state = NodeState.Failure;
            }
        }
        else
        {
            if (controller.IsAttacking())
            {
                state = NodeState.Running;
            }
            else
            {
                attackStarted = false;
                state = NodeState.Success;
            }
        }

        return state;
    }

    public override void Reset()
    {
        attackStarted = false;
        base.Reset();
    }
}