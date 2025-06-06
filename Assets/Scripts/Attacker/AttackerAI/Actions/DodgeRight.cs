using UnityEngine;
using BehaviorTree;

public class DodgeRight : ActionNode
{
    public DodgeRight(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("DodgeRight: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 오른쪽으로 회피 실행
        bool dodgeExecuted = controller.Dodge(Vector3.right);

        if (dodgeExecuted)
        {
            Debug.Log($"DodgeRight: Right dodge executed successfully");
            return NodeState.Success;
        }
        else
        {
            Debug.Log($"DodgeRight: Right dodge failed - cannot dodge right now");
            return NodeState.Failure;
        }
    }
}