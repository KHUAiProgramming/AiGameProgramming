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
        // 방어형 AI 전략: 확정적 방어 + 선택적 공격
        SelectorNode rootSelector = new SelectorNode(
            // 1. 생존 우선 - HP 낮으면 즉시 회피 (확정)
            new SequenceNode(
                new DefenderAI.IsLowHP(this, blackboard, 0.4f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 2. 근거리 필수 방어 - 가까우면 무조건 방어 (확정)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.8f),
                new DefenderAI.CanBlock(this, blackboard),
                new DefenderAI.BlockAction(this, blackboard)
            ),

            // 3. 중거리 방어 - 접근하면 방어 (확정)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.5f),
                new DefenderAI.CanBlock(this, blackboard),
                new DefenderAI.BlockAction(this, blackboard)
            ),

            // 4. 반격 기회 - 공격 가능할 때만 (50% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.2f),
                new DefenderAI.CanAttack(this, blackboard),
                new RandomDecorator(0.5f,  // 핵심: 공격 타이밍 조절
                    new DefenderAI.AttackAction(this, blackboard)
                )
            ),

            // 5. 위험 회피 - 너무 가까우면 회피 (확정)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.2f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 6. 긴급 상황 - 가장자리 탈출 (확정)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.5f),
                new DefenderAI.MoveToOpenSpace(this, blackboard)
            ),

            // 7. 중거리 행동 - 포지셔닝 선택 (랜덤)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.0f),
                new RandomSelector(  // 핵심: 행동 패턴 다양화
                    new DefenderAI.MoveSideways(this, blackboard),
                    new DefenderAI.PatrolOrWait(this, blackboard, 3.2f, 0.5f, 0.4f)
                )
            ),

            // 8. 조심스럽게 접근 - 너무 멀면 접근 (확정)
            new SequenceNode(
                new DefenderAI.IsFarFromTarget(this, blackboard, 4.0f),
                new DefenderAI.MoveToTarget(this, blackboard, 2.5f)
            ),

            // 9. 기본: 안전 거리 유지 (확정)
            new DefenderAI.PatrolOrWait(this, blackboard, 3.5f, 0.6f, 0.3f)
        );

        SetRootNode(rootSelector);
    }
}