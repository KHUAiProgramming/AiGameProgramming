using UnityEngine;
using BehaviorTree;

public class IsAttacking : ConditionNode
{
    public IsAttacking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("IsAttacking: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        bool isAttacking = controller.IsAttacking;
        Debug.Log($"IsAttacking: {isAttacking}");

        return isAttacking ? NodeState.Success : NodeState.Failure;
    }
}