using UnityEngine;
using BehaviorTree;

public class StayPosition : ActionNode
{
    public StayPosition(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        
        if (controller == null)
        {
            state = NodeState.Failure;
            return state;
        }
        
        // 현재 위치에서 정지
        controller.Stop();
        state = NodeState.Success;
        
        Debug.Log("StayPosition: Staying at current position");
        return state;
    }
}