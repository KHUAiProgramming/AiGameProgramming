using UnityEngine;
using BehaviorTree;

public class ExecuteDodge : ActionNode
{
    private bool dodgeStarted = false;
    private float dodgeProbability;
    private ActionNode fallbackAction;

    public ExecuteDodge(MonoBehaviour owner, Blackboard blackboard, float probability = 0.75f, ActionNode fallback = null)
        : base(owner, blackboard)
    {
        dodgeProbability = probability;
        fallbackAction = fallback;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            state = NodeState.Failure;
            return state;
        }

        if (!dodgeStarted)
        {
            // 확률 계산으로 대체 액션 실행
            if (Random.Range(0f, 1f) > dodgeProbability && fallbackAction != null)
            {
                Debug.Log($"ExecuteDodge: Using fallback action (probability: {dodgeProbability:P})");
                return fallbackAction.Evaluate();
            }

            if (!controller.CanDodge())
            {
                state = NodeState.Failure;
                return state;
            }

            // 가장 적절한 회피 방향 선택
            Vector3[] dodgeDirections = { Vector3.left, Vector3.right, Vector3.back };
            Vector3 selectedDirection = dodgeDirections[Random.Range(0, dodgeDirections.Length)];

            bool success = controller.Dodge(selectedDirection);
            if (success)
            {
                dodgeStarted = true;
                state = NodeState.Running;
                Debug.Log($"ExecuteDodge: Started dodge in {selectedDirection} direction");
            }
            else
            {
                state = NodeState.Failure;
            }
        }
        else
        {
            // 회피 진행 중 확인
            if (controller.IsDodging)
            {
                state = NodeState.Running;
            }
            else
            {
                dodgeStarted = false;
                state = NodeState.Success;
                Debug.Log("ExecuteDodge: Dodge completed successfully");
            }
        }

        return state;
    }

    public override void Reset()
    {
        dodgeStarted = false;
        base.Reset();
    }
}