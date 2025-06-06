using UnityEngine;
using BehaviorTree;

public class IsEarlyStage : ConditionNode
{
    public IsEarlyStage(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        // opponent.canBeCountered가 true인지 확인 (17% 윈도우)
        Transform target = blackboard.GetValue<Transform>("target");
        if (target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        bool isEarly = false;

        // AttackerController인 경우
        AttackerController opponentAttacker = target.GetComponent<AttackerController>();
        if (opponentAttacker != null)
        {
            isEarly = opponentAttacker.CanBeCountered;
        }

        // DefenderController인 경우
        DefenderController opponentDefender = target.GetComponent<DefenderController>();
        if (opponentDefender != null)
        {
            isEarly = opponentDefender.CanBeCountered;
        }

        state = isEarly ? NodeState.Success : NodeState.Failure;

        if (isEarly)
        {
            Debug.Log("IsEarlyStage: Opponent is in counterable stage (17% window)");
        }

        return state;
    }
}