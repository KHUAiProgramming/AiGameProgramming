using UnityEngine;
using BehaviorTree;

public class ExecuteBlock : ActionNode
{
    private bool blockStarted = false;
    private float blockProbability;
    private ActionNode fallbackAction;

    public ExecuteBlock(MonoBehaviour owner, Blackboard blackboard, float probability = 0.75f, ActionNode fallback = null)
        : base(owner, blackboard)
    {
        blockProbability = probability;
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

        if (!blockStarted)
        {
            // 확률 계산으로 대체 액션 실행
            if (Random.Range(0f, 1f) > blockProbability && fallbackAction != null)
            {
                Debug.Log($"ExecuteBlock: Using fallback action (probability: {blockProbability:P})");
                return fallbackAction.Evaluate();
            }

            if (!controller.CanBlock())
            {
                state = NodeState.Failure;
                return state;
            }

            bool success = controller.Block();
            if (success)
            {
                blockStarted = true;
                state = NodeState.Running;
                Debug.Log("ExecuteBlock: Started blocking");
            }
            else
            {
                state = NodeState.Failure;
            }
        }
        else
        {
            // 방어 진행 중 확인
            if (controller.IsBlocking)
            {
                state = NodeState.Running;
            }
            else
            {
                blockStarted = false;
                state = NodeState.Success;
                Debug.Log("ExecuteBlock: Block completed successfully");
            }
        }

        return state;
    }

    public override void Reset()
    {
        blockStarted = false;
        base.Reset();
    }
}