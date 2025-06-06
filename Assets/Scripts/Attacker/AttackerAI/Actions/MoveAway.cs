using UnityEngine;
using BehaviorTree;

public class MoveAway : ActionNode
{
    private float moveAwayDistance;

    public MoveAway(MonoBehaviour owner, Blackboard blackboard, float distance = 3.0f) : base(owner, blackboard)
    {
        this.moveAwayDistance = distance;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        Transform target = blackboard.GetValue<Transform>("target");

        if (controller == null)
        {
            Debug.LogError("MoveAway: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        if (target == null)
        {
            Debug.LogWarning("MoveAway: No target found in blackboard");
            return NodeState.Failure;
        }

        Vector3 ownerPosition = owner.transform.position;
        Vector3 targetPosition = target.position;
        float currentDistance = Vector3.Distance(ownerPosition, targetPosition);

        // 이미 충분히 멀리 있으면 성공
        if (currentDistance >= moveAwayDistance)
        {
            controller.Stop();
            Debug.Log($"MoveAway: Already at safe distance ({currentDistance:F2}m >= {moveAwayDistance}m)");
            return NodeState.Success;
        }        // 타겟 반대 방향으로 이동 (4방향 제한)
        Vector3 directionAway = (ownerPosition - targetPosition).normalized;
        controller.Move(directionAway);

        // Rigidbody velocity 설정
        Rigidbody rb = owner.GetComponent<Rigidbody>();
        float moveSpeed = blackboard.GetValue<float>("moveSpeed");
        rb.velocity = new Vector3(directionAway.x * moveSpeed, rb.velocity.y, directionAway.z * moveSpeed);

        Debug.Log($"MoveAway: Moving away from target (Distance: {currentDistance:F2}m, Target: {moveAwayDistance}m)");
        return NodeState.Running;
    }
}