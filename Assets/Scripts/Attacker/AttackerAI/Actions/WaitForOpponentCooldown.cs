using UnityEngine;
using BehaviorTree;

public class WaitForOpponentCooldown : ActionNode
{
    private float minimumCooldown;

    public WaitForOpponentCooldown(MonoBehaviour owner, Blackboard blackboard, float minimumCooldown = 1.0f) : base(owner, blackboard)
    {
        this.minimumCooldown = minimumCooldown;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");
        AttackerController opponent = blackboard.GetValue<AttackerController>("opponent");

        if (controller == null)
        {
            Debug.LogError("WaitForOpponentCooldown: AttackerController not found in blackboard");
            return NodeState.Failure;
        }

        if (opponent == null)
        {
            Debug.LogWarning("WaitForOpponentCooldown: Opponent not found in blackboard");
            return NodeState.Failure;
        }

        float opponentCooldown = opponent.AttackCooldownRemaining;

        // 상대방 쿨타임이 충분하면 성공
        if (opponentCooldown >= minimumCooldown)
        {
            Debug.Log($"WaitForOpponentCooldown: Opponent has sufficient cooldown ({opponentCooldown:F2}s >= {minimumCooldown}s)");
            return NodeState.Success;
        }

        // 대기 중에는 안전거리 유지
        controller.Stop();
        Debug.Log($"WaitForOpponentCooldown: Waiting for opponent cooldown ({opponentCooldown:F2}s < {minimumCooldown}s)");
        return NodeState.Running;
    }
}