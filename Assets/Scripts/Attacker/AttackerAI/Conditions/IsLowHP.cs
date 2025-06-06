using UnityEngine;
using BehaviorTree;

public class IsLowHP : ConditionNode
{
    private float lowHPThreshold;

    public IsLowHP(MonoBehaviour owner, Blackboard blackboard, float threshold = 0.3f)
        : base(owner, blackboard)
    {
        lowHPThreshold = threshold;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        if (controller == null)
        {
            state = NodeState.Failure;
            return state;
        }

        bool isLowHP = controller.HPPercentage < lowHPThreshold;
        state = isLowHP ? NodeState.Success : NodeState.Failure;

        if (isLowHP)
        {
            Debug.Log($"IsLowHP: HP is low ({controller.HPPercentage:P} < {lowHPThreshold:P})");
        }

        return state;
    }
}