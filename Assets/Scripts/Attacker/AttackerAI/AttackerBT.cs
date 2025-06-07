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
        // 공격형 AI 전략: 단순한 거리 + 확률 기반
        SelectorNode rootSelector = new SelectorNode(
            // 1. HP 낮으면 회피 우선
            new SequenceNode(
                new AttackerAI.IsLowHP(this, blackboard, 0.3f),
                new AttackerAI.CanDodge(this, blackboard),
                new AttackerAI.DodgeAway(this, blackboard)
            ),

            // 2. 근거리에서 공격 가능하면 공격 (70% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 1.5f),
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.7f),
                new AttackerAI.AttackAction(this, blackboard)
            ),

            // 3. 중거리에서 공격 가능하면 공격 (40% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 2.5f),
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.4f),
                new AttackerAI.AttackAction(this, blackboard)
            ),

            // 4. 가까이 있고 회피 가능하면 회피 (20% 확률)
            new SequenceNode(
                new AttackerAI.IsInRange(this, blackboard, 1.0f),
                new AttackerAI.CanDodge(this, blackboard),
                new AttackerAI.RandomChance(this, blackboard, 0.2f),
                new AttackerAI.DodgeAway(this, blackboard)
            ),

            // 5. 기본: 접근
            new AttackerAI.MoveToTarget(this, blackboard)
        );

        SetRootNode(rootSelector);
    }
}