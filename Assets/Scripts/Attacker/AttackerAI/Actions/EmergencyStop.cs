using UnityEngine;
using BehaviorTree;

public class EmergencyStop : ActionNode
{
    public EmergencyStop(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("EmergencyStop: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 즉시 모든 행동 중단
        controller.Stop();

        // 진행 중인 모든 코루틴 중단 (만약 가능하다면)
        if (controller is MonoBehaviour mb)
        {
            mb.StopAllCoroutines();
        }

        Debug.Log("EmergencyStop: All actions stopped immediately");
        return NodeState.Success;
    }
}