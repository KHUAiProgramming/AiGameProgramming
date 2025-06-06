using UnityEngine;
using BehaviorTree;

public class DodgeBackward : ActionNode
{
    public DodgeBackward(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("DodgeBackward: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 뒤쪽으로 회피 실행
        bool dodgeExecuted = controller.Dodge(Vector3.back);

        if (dodgeExecuted)
        {
            Debug.Log($"DodgeBackward: Backward dodge executed successfully");
            return NodeState.Success;
        }
        else
        {
            Debug.Log($"DodgeBackward: Backward dodge failed - cannot dodge right now");
            return NodeState.Failure;
        }
    }
}