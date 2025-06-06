using UnityEngine;
using BehaviorTree;

public class WaitForSafeTiming : ActionNode
{
    public WaitForSafeTiming(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (controller == null)
        {
            Debug.LogError("WaitForSafeTiming: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        if (opponent == null)
        {
            Debug.LogWarning("WaitForSafeTiming: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        // 안전한 타이밍 조건들
        bool opponentNotAttacking = !opponent.IsAttacking;
        bool opponentNotCounterable = !opponent.CanBeCountered;
        bool opponentOnCooldown = opponent.AttackCooldownRemaining > 0.5f;

        bool isSafeTiming = opponentNotAttacking && opponentNotCounterable && opponentOnCooldown;

        if (isSafeTiming)
        {
            Debug.Log("WaitForSafeTiming: Safe timing achieved - opponent not threatening");
            return NodeState.Success;
        }

        // 안전하지 않으면 대기하며 안전거리 유지
        controller.Stop();
        Debug.Log($"WaitForSafeTiming: Waiting for safe timing (Attacking: {opponent.IsAttacking}, Counterable: {opponent.CanBeCountered}, Cooldown: {opponent.AttackCooldownRemaining:F2}s)");
        return NodeState.Running;
    }
}