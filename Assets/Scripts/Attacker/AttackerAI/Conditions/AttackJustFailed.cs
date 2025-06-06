using UnityEngine;
using BehaviorTree;

public class AttackJustFailed : ConditionNode
{
    public AttackJustFailed(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        // Blackboard에서 마지막 공격 상태 확인
        bool lastAttackFailed = blackboard.GetValue<bool>("lastAttackFailed");

        state = lastAttackFailed ? NodeState.Success : NodeState.Failure;

        if (lastAttackFailed)
        {
            Debug.Log("AttackJustFailed: Previous attack failed, need to be careful of counter");
            // 플래그 리셋
            blackboard.SetValue("lastAttackFailed", false);
        }

        return state;
    }
}