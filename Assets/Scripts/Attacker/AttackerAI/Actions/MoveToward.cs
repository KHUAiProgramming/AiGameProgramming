using UnityEngine;
using BehaviorTree;

public class MoveToward : ActionNode
{
    private float targetDistance;

    public MoveToward(MonoBehaviour owner, Blackboard blackboard, float distance = 2.0f) : base(owner, blackboard)
    {
        this.targetDistance = distance;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        Transform target = blackboard.GetValue<Transform>("target");

        if (controller == null)
        {
            Debug.LogError("MoveToward: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        if (target == null)
        {
            Debug.LogWarning("MoveToward: No target found in blackboard");
            return NodeState.Failure;
        }

        Vector3 ownerPosition = owner.transform.position;
        Vector3 targetPosition = target.position;
        float currentDistance = Vector3.Distance(ownerPosition, targetPosition);

        // 목표 거리에 도달했으면 성공
        if (currentDistance <= targetDistance)
        {
            controller.Stop();
            Debug.Log($"MoveToward: Reached target distance ({currentDistance:F2}m <= {targetDistance}m)");
            return NodeState.Success;
        }

        // 타겟 방향으로 이동
        Vector3 directionToward = (targetPosition - ownerPosition).normalized;
        controller.Move(directionToward);

        // Rigidbody velocity 설정
        Rigidbody rb = owner.GetComponent<Rigidbody>();
        float moveSpeed = blackboard.GetValue<float>("moveSpeed");
        rb.velocity = new Vector3(directionToward.x * moveSpeed, rb.velocity.y, directionToward.z * moveSpeed);

        Debug.Log($"MoveToward: Moving toward target (Distance: {currentDistance:F2}m, Target: {targetDistance}m)");
        return NodeState.Running;
    }
}