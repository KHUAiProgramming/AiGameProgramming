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
        SelectorNode rootSelector = new SelectorNode(
            // 1. HP 낮으면 회피 우선 (<=25%)
            new SequenceNode(
                new AttackerAI.IsLowHP(this, blackboard, 0.25f),
                new AttackerAI.CanDodge(this, blackboard),
                new AttackerAI.DodgeAway(this, blackboard)
            ),

            // 2. 가까이 있으면 즉시 공격
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 1.8f),
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.AttackAction(this, blackboard)
            ),

            // 3. 중거리에서 공격 (75% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 2.8f),
                new AttackerAI.CanAttack(this, blackboard),
                new RandomDecorator(0.75f,
                    new AttackerAI.AttackAction(this, blackboard)
                )
            ),

            // 4. 위험할 때 방어
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 2.0f),
                new AttackerAI.CanBlock(this, blackboard),
                new AttackerAI.BlockAction(this, blackboard)
            ),

            // 5. 너무 가까우면 회피 (30% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 1.0f),
                new AttackerAI.CanDodge(this, blackboard),
                new RandomDecorator(0.3f,
                    new AttackerAI.DodgeAway(this, blackboard)
                )
            ),

            // 6. 3미터 이상 -> 접근
            new SequenceNode(
                new AttackerAI.IsFarFromTarget(this, blackboard, 3.0f),
                new AttackerAI.MoveToTarget(this, blackboard)
            ),

            // 7. 기본 - 접근
            new AttackerAI.MoveToTarget(this, blackboard)
        );

        SetRootNode(rootSelector);
    }
}