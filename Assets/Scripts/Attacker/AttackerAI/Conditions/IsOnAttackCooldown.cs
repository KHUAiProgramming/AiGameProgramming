using UnityEngine;
using BehaviorTree;

public class IsOnAttackCooldown : ConditionNode
{
    public IsOnAttackCooldown(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            state = NodeState.Failure;
            return state;
        }

        bool onCooldown = controller.AttackCooldownRemaining > 0;
        state = onCooldown ? NodeState.Success : NodeState.Failure;

        if (onCooldown)
        {
            Debug.Log($"IsOnAttackCooldown: Attack on cooldown ({controller.AttackCooldownRemaining:F1}s remaining)");
        }

        return state;
    }
}