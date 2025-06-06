using UnityEngine;
using BehaviorTree;

public class BlockAction : ActionNode
{
    public BlockAction(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("BlockAction: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 방어 실행
        bool blockExecuted = controller.Block();

        if (blockExecuted)
        {
            Debug.Log($"BlockAction: Block executed successfully");
            return NodeState.Success;
        }
        else
        {
            Debug.Log($"BlockAction: Block failed - cannot block right now");
            return NodeState.Failure;
        }
    }
}