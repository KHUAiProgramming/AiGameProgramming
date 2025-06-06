using UnityEngine;
using BehaviorTree;

public class IsDodging : ConditionNode
{
    public IsDodging(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("IsDodging: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        bool isDodging = controller.IsDodging;
        Debug.Log($"IsDodging: {isDodging}");

        return isDodging ? NodeState.Success : NodeState.Failure;
    }
}