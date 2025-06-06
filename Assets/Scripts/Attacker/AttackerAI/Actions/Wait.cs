using UnityEngine;
using BehaviorTree;
using System.Collections;

public class Wait : ActionNode
{
    private float waitTime;
    private float startTime;
    private bool isWaiting;

    public Wait(MonoBehaviour owner, Blackboard blackboard, float waitTime = 1.0f) : base(owner, blackboard)
    {
        this.waitTime = waitTime;
        this.isWaiting = false;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
        {
            Debug.LogError("Wait: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        // 대기 시작
        if (!isWaiting)
        {
            startTime = Time.time;
            isWaiting = true;
            controller.Stop(); // 대기 중에는 이동 중단
            Debug.Log($"Wait: Started waiting for {waitTime} seconds");
        }

        // 대기 시간 체크
        float elapsedTime = Time.time - startTime;
        if (elapsedTime >= waitTime)
        {
            isWaiting = false;
            Debug.Log($"Wait: Wait completed ({elapsedTime:F2}s)");
            return NodeState.Success;
        }

        Debug.Log($"Wait: Waiting... ({elapsedTime:F2}s / {waitTime}s)");
        return NodeState.Running;
    }
}