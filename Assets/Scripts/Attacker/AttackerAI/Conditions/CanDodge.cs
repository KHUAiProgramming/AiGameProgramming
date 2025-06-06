using UnityEngine;
using BehaviorTree;

public class CanDodge : ConditionNode
{
    public CanDodge(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("CanDodge: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        bool canDodge = controller.CanDodge();
        Debug.Log($"CanDodge: {canDodge}");

        return canDodge ? NodeState.Success : NodeState.Failure;
    }
}