using UnityEngine;
using BehaviorTree;

public class MaintainDistance : ActionNode
{
    private float targetDistance;
    private float tolerance;

    public MaintainDistance(MonoBehaviour owner, Blackboard blackboard, float distance = 4.0f, float tolerance = 0.5f) : base(owner, blackboard)
    {
        this.targetDistance = distance;
        this.tolerance = tolerance;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        Transform target = blackboard.GetValue<Transform>("target");

        if (controller == null)
        {
            Debug.LogError("MaintainDistance: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        if (target == null)
        {
            Debug.LogWarning("MaintainDistance: No target found in blackboard");
            return NodeState.Failure;
        }

        Vector3 ownerPosition = owner.transform.position;
        Vector3 targetPosition = target.position;
        float currentDistance = Vector3.Distance(ownerPosition, targetPosition);

        // 목표 거리 범위 내에 있으면 성공
        if (Mathf.Abs(currentDistance - targetDistance) <= tolerance)
        {
            controller.Stop();
            Debug.Log($"MaintainDistance: At optimal distance ({currentDistance:F2}m ≈ {targetDistance}m ± {tolerance}m)");
            return NodeState.Success;
        }        // 너무 가까우면 멀어지기
        if (currentDistance < targetDistance - tolerance)
        {
            Vector3 directionAway = (ownerPosition - targetPosition).normalized;
            controller.Move(directionAway);

            // Rigidbody velocity 설정
            Rigidbody rb = owner.GetComponent<Rigidbody>();
            float moveSpeed = blackboard.GetValue<float>("moveSpeed");
            rb.velocity = new Vector3(directionAway.x * moveSpeed, rb.velocity.y, directionAway.z * moveSpeed);

            Debug.Log($"MaintainDistance: Too close, moving away ({currentDistance:F2}m < {targetDistance}m)");
        }
        // 너무 멀면 접근하기
        else if (currentDistance > targetDistance + tolerance)
        {
            Vector3 directionToward = (targetPosition - ownerPosition).normalized;
            controller.Move(directionToward);

            // Rigidbody velocity 설정
            Rigidbody rb = owner.GetComponent<Rigidbody>();
            float moveSpeed = blackboard.GetValue<float>("moveSpeed");
            rb.velocity = new Vector3(directionToward.x * moveSpeed, rb.velocity.y, directionToward.z * moveSpeed);

            Debug.Log($"MaintainDistance: Too far, moving closer ({currentDistance:F2}m > {targetDistance}m)");
        }

        return NodeState.Running;
    }
}