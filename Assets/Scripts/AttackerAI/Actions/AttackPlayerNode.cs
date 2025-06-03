using UnityEngine;
using BehaviorTree;
public class AttackPlayerNode : ActionNode
{
    private float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    private Animator animator;

    public AttackPlayerNode(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
    {
        animator = owner.GetComponentInChildren<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (!blackboard.HasKey("target"))
        {
            state = NodeState.Failure;
            return state;
        }

        Transform target = blackboard.GetValue<Transform>("target");

        if (Time.time - lastAttackTime < attackCooldown)
        {
            state = NodeState.Running;
            return state;
        }

        Vector3 lookDirection = (target.transform.position - owner.transform.position).normalized;
        owner.transform.rotation = Quaternion.LookRotation(lookDirection);

        if (animator != null)
        {
            animator.SetTrigger("onAttack");
        }

        lastAttackTime = Time.time;
        state = NodeState.Success;
        return state;
    }
}
