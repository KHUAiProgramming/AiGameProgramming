using UnityEngine;
using BehaviorTree;
public class AttackerBT : BehaviorTree.BehaviorTree
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 4f;

    private Blackboard blackboard;

    void Awake()
    {
        blackboard = new Blackboard();
        InitializeBlackboard();
    }

    void InitializeBlackboard()
    {
        blackboard.SetValue("self", this);
        blackboard.SetValue("moveSpeed", moveSpeed);

        AttackerController controller = GetComponent<AttackerController>();
        blackboard.SetValue("controller", controller);
        blackboard.SetValue("target", target);

        // Controller에 target 정보 전달
        if (controller != null && target != null)
        {
            controller.SetTarget(target);
        }
    }

    protected override void OnUpdate()
    {
        blackboard.SetValue("target", target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        blackboard.SetValue("target", target);
    }

    protected override void ConstructTree()
    {
        // 공격형 AI 전략: 단순하고 공격적인 전투
        SelectorNode rootSelector = new SelectorNode(
            // 1. HP 낮으면 회피 우선 (25% 이하)
            new SequenceNode(
                new AttackerAI.IsLowHP(this, blackboard, 0.25f),
                new AttackerAI.CanDodge(this, blackboard),
                new AttackerAI.DodgeAway(this, blackboard)
            ),

            // 2. 위험할 때 방어 (50% 확률로 증가)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 2.0f), // 2미터 이내에서
                new AttackerAI.CanBlock(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.5f), // 50% 확률
                new AttackerAI.BlockAction(this, blackboard)
            ),

            // 3. 아주 가까이 있으면 즉시 공격 (90% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 1.8f),
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.9f),
                new AttackerAI.AttackAction(this, blackboard)
            ),

            // 4. 중거리에서 적극적 공격 (75% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 2.8f),
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.75f),
                new AttackerAI.AttackAction(this, blackboard)
            ),

            // 5. 너무 가까우면 회피 (40% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 1.0f),
                new AttackerAI.CanDodge(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.4f),
                new AttackerAI.DodgeAway(this, blackboard)
            ),

            // 6. 멀리 있으면 적극적 접근 (3미터 이상)
            new SequenceNode(
                new AttackerAI.IsFarFromTarget(this, blackboard, 3.0f),
                new AttackerAI.MoveToTarget(this, blackboard)
            ),

            // 7. 기본: 계속 접근
            new AttackerAI.MoveToTarget(this, blackboard)
        );

        SetRootNode(rootSelector);
    }
}