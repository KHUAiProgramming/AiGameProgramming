using UnityEngine;
using BehaviorTree;

public class StopMoving : ActionNode
{
    public StopMoving(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("StopMoving: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 이동 중단
        controller.Stop();
        Debug.Log($"StopMoving: Movement stopped");

        return NodeState.Success;
    }
}