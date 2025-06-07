using UnityEngine;
using BehaviorTree;

public class DefenderBT : BehaviorTree.BehaviorTree
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform groundObject; // 바닥 오브젝트 참조

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
        blackboard.SetValue("groundObject", groundObject); // 바닥 오브젝트 등록

        DefenderController controller = GetComponent<DefenderController>();
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
        // 방어형 AI 전략: 균형잡힌 방어적 전투
        SelectorNode rootSelector = new SelectorNode(
            // 1. 생존 모드 - HP 낮으면 회피 우선
            new SequenceNode(
                new DefenderAI.IsLowHP(this, blackboard, 0.3f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 2. 방어 우선 - 근거리에서 방어 (60% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.8f),
                new DefenderAI.CanBlock(this, blackboard),
                new DefenderAI.RandomChance(this, blackboard, 0.6f),
                new DefenderAI.BlockAction(this, blackboard)
            ),

            // 3. 공격 기회 - 공격 가능할 때 (50% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.0f),
                new DefenderAI.CanAttack(this, blackboard),
                new DefenderAI.RandomChance(this, blackboard, 0.5f),
                new DefenderAI.AttackAction(this, blackboard)
            ),

            // 4. 회피 - 너무 가까우면 회피 (50% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.2f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.RandomChance(this, blackboard, 0.5f),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 5. 긴급 상황 - 가장자리에 몰렸으면 중앙으로
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.0f),
                new DefenderAI.MoveToOpenSpace(this, blackboard)
            ),

            // 6. 측면 이동 - 중거리에서 포지셔닝
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.0f),
                new DefenderAI.MoveSideways(this, blackboard)
            ),

            // 7. 접근 - 너무 멀면 조심스럽게 접근
            new SequenceNode(
                new DefenderAI.IsFarFromTarget(this, blackboard, 4.0f),
                new DefenderAI.MoveToTarget(this, blackboard)
            ),

            // 8. 기본: 적절한 거리 유지
            new DefenderAI.PatrolOrWait(this, blackboard)
        );

        SetRootNode(rootSelector);
    }
}