using UnityEngine;
using BehaviorTree;
using System;
public class MoveToTarget : ActionNode
{
    public MoveToTarget(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Transform enemy = blackboard.GetValue<Transform>("target");
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        Rigidbody rb = self.GetComponent<Rigidbody>();
        float speed = blackboard.GetValue<float>("moveSpeed");

        Vector3 direction = (enemy.position - self.transform.position).normalized;
        float distance = Vector3.Distance(self.transform.position, enemy.position);

        if (distance <= 1f)
        {
            controller.Stop();
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return NodeState.Success;
        }

        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);

        controller.Move(direction);

        return NodeState.Running;
    }

    public override void Reset()
    {
        MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
        Rigidbody rb = self.GetComponent<Rigidbody>();
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        controller?.Stop();
        if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
        base.Reset();
    }
}