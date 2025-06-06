using UnityEngine;
using BehaviorTree;

public class MaintainSafeDistance : ActionNode
{
    private float targetDistance;
    private float tolerance = 0.5f;

    public MaintainSafeDistance(MonoBehaviour owner, Blackboard blackboard, float distance = 3.5f)
        : base(owner, blackboard)
    {
        targetDistance = distance;
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

        float currentDistance = Vector3.Distance(controller.transform.position, target.position);

        if (Mathf.Abs(currentDistance - targetDistance) <= tolerance)
        {
            // 이미 안전거리에 있음
            controller.Stop();
            state = NodeState.Success;
            Debug.Log($"MaintainSafeDistance: Already at safe distance ({currentDistance:F1}m)");
        }
        else if (currentDistance < targetDistance)
        {
            // 너무 가까움 - 후퇴 (4방향 제한)
            Vector3 awayDirection = (controller.transform.position - target.position).normalized;
            controller.Move(awayDirection);

            // Rigidbody velocity 설정
            Rigidbody rb = controller.GetComponent<Rigidbody>();
            float moveSpeed = blackboard.GetValue<float>("moveSpeed");
            rb.velocity = new Vector3(awayDirection.x * moveSpeed, rb.velocity.y, awayDirection.z * moveSpeed);

            state = NodeState.Running;
            Debug.Log($"MaintainSafeDistance: Moving away to maintain safe distance ({currentDistance:F1}m -> {targetDistance}m)");
        }
        else
        {
            // 너무 멀음 - 접근 (4방향 제한)
            Vector3 towardDirection = (target.position - controller.transform.position).normalized;
            controller.Move(towardDirection);

            // Rigidbody velocity 설정
            Rigidbody rb = controller.GetComponent<Rigidbody>();
            float moveSpeed = blackboard.GetValue<float>("moveSpeed");
            rb.velocity = new Vector3(towardDirection.x * moveSpeed, rb.velocity.y, towardDirection.z * moveSpeed);

            state = NodeState.Running;
            Debug.Log($"MaintainSafeDistance: Moving closer to maintain safe distance ({currentDistance:F1}m -> {targetDistance}m)");
        }

        return state;
    }
}