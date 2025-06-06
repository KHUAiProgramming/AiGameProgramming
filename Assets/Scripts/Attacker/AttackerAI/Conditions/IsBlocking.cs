using UnityEngine;
using BehaviorTree;

public class IsBlocking : ConditionNode
{
    public IsBlocking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("IsBlocking: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        bool isBlocking = controller.IsBlocking;
        Debug.Log($"IsBlocking: {isBlocking}");

        return isBlocking ? NodeState.Success : NodeState.Failure;
    }
}