using UnityEngine;
using BehaviorTree;

public class MoveToTargetNode : ActionNode
{
    private float moveSpeed;
    private float stopDistance;

    public MoveToTargetNode(MonoBehaviour owner, Blackboard blackboard,
                           float moveSpeed = 3f, float stopDistance = 1.5f)
        : base(owner, blackboard)
    {
        this.moveSpeed = moveSpeed;
        this.stopDistance = stopDistance;
    }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Rigidbody rigid = self.GetComponent<Rigidbody>();
        Transform target = blackboard.GetValue<Transform>("target");

        if (self.transform == null || target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        Vector3 targetPosition = target.position;
        Vector3 currentPosition = self.transform.position;
        float distance = Vector3.Distance(currentPosition, targetPosition);

        Debug.Log("distance: " + distance);
        Debug.Log("stopDistance: " + stopDistance);

        if (distance <= stopDistance)
        {
            state = NodeState.Success;
            return state;
        }

        Vector3 direction = (targetPosition - currentPosition).normalized;

        Debug.Log("direction: " + direction);

        direction.y = 0;

        if (direction == Vector3.zero)
        {
            state = NodeState.Success;
            return state;
        }

        Move(rigid, direction);

        RotateTowards(self.transform, direction);

        state = NodeState.Running;
        return state;
    }

    private void Move(Rigidbody rb, Vector3 direction)
    {
        if (rb != null)
        {
            Vector3 movement = direction * moveSpeed;
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
            Debug.Log("rigidbody.velocity" + rb.velocity);
        }
    }

    private void RotateTowards(Transform transform, Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float rotationSpeed = blackboard.GetValue<float>("rotationSpeed");
            if (rotationSpeed <= 0) rotationSpeed = 5f;

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}