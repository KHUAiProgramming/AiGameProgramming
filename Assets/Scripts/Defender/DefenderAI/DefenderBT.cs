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
        SelectorNode rootSelector = new SelectorNode(
            // 0. 죽으면 아무것도 하지 않음 (최우선)
            new DefenderAI.IsDead(this, blackboard),

            // 1. HP 낮으면 즉시 회피
            new SequenceNode(
                new DefenderAI.IsLowHP(this, blackboard, 0.4f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 2. 진짜 반격 - 방어 직후 즉시 반격
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.2f),
                new DefenderAI.CanAttack(this, blackboard),

                new RandomDecorator(0.5f,
                    new DefenderAI.CounterAfterBlock(this, blackboard)
                )
            ),

            // 3. 가까우면 무조건 방어
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.8f),
                new DefenderAI.CanBlock(this, blackboard),
                new DefenderAI.BlockAction(this, blackboard)
            ),

            // 5. 일반 공격 확률을 더 낮춤
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.5f),
                new ProbabilitySelector(0.15f,
                    new SequenceNode(
                        new DefenderAI.CanAttack(this, blackboard),
                        new DefenderAI.AttackAction(this, blackboard)
                    ),
                    new SequenceNode(
                        new DefenderAI.CanBlock(this, blackboard),
                        new DefenderAI.BlockAction(this, blackboard)
                    )
                )
            ),

            // 5. 너무 가까우면 회피
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.2f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 6. 가장자리 탈출
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.5f),
                new DefenderAI.MoveToOpenSpace(this, blackboard)
            ),

            // 7. 포지셔닝 선택 (랜덤)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.0f),
                new DefenderAI.MoveSideways(this, blackboard)
            ),

            // 8. 너무 멀면 접근 
            new SequenceNode(
                new DefenderAI.IsFarFromTarget(this, blackboard, 4.0f),
                new DefenderAI.MoveToTarget(this, blackboard, 2.5f)
            ),

            // 9. 기본 거리 유지
            new DefenderAI.PatrolOrWait(this, blackboard, 3.5f, 0.6f, 0.3f)
        );

        SetRootNode(rootSelector);
    }
}