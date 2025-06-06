using UnityEngine;
using BehaviorTree;

public class DodgeLeft : ActionNode
{
    public DodgeLeft(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("DodgeLeft: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 왼쪽으로 회피 실행
        bool dodgeExecuted = controller.Dodge(Vector3.left);

        if (dodgeExecuted)
        {
            Debug.Log($"DodgeLeft: Left dodge executed successfully");
            return NodeState.Success;
        }
        else
        {
            Debug.Log($"DodgeLeft: Left dodge failed - cannot dodge right now");
            return NodeState.Failure;
        }
    }
}