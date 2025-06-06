using UnityEngine;
using BehaviorTree;

public class CanBlock : ConditionNode
{
    public CanBlock(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("CanBlock: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        bool canBlock = controller.CanBlock();
        Debug.Log($"CanBlock: {canBlock}");

        return canBlock ? NodeState.Success : NodeState.Failure;
    }
}