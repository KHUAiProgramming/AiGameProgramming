using UnityEngine;
using BehaviorTree;

public class IsCloseRange : ConditionNode
{
    private float closeRange;
    
    public IsCloseRange(MonoBehaviour owner, Blackboard blackboard, float range = 3f) 
        : base(owner, blackboard) 
    {
        closeRange = range;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        Transform target = blackboard.GetValue<Transform>("target");
        
        if (controller == null || target == null)
        {
            state = NodeState.Failure;
            return state;
        }
        
        float distance = Vector3.Distance(controller.transform.position, target.position);
        bool isClose = distance <= closeRange;
        
        state = isClose ? NodeState.Success : NodeState.Failure;
        
        if (isClose)
        {
            Debug.Log($"IsCloseRange: Target is close ({distance:F1}m <= {closeRange}m)");
        }
        
        return state;
    }
}