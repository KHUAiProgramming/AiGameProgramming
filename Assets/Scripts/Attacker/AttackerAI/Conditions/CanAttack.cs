using UnityEngine;
using BehaviorTree;

public class CanAttack : ConditionNode
{
    public CanAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("CanAttack: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        bool canAttack = controller.CanAttack();
        Debug.Log($"CanAttack: {canAttack}");

        return canAttack ? NodeState.Success : NodeState.Failure;
    }
}