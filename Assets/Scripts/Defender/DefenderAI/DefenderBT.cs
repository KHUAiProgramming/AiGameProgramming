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
        // 승리 지향적 방어형 AI - 전투 최우선
        SelectorNode rootSelector = new SelectorNode(
            // 1. 생존 모드 - HP 낮으면 회피 우선
            new SequenceNode(
                new DefenderAI.IsLowHP(this, blackboard, 0.25f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 2. 공격 - 근거리/중거리에서 공격 기회 (60% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.5f),
                new DefenderAI.CanAttack(this, blackboard),
                new DefenderAI.RandomChance(this, blackboard, 0.6f),
                new DefenderAI.AttackAction(this, blackboard)
            ),

            // 3. 방어 - 공격 불가능할 때만 방어 (80% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.5f),
                new DefenderAI.CanBlock(this, blackboard),
                new DefenderAI.RandomChance(this, blackboard, 0.8f),
                new DefenderAI.BlockAction(this, blackboard)
            ),

            // 4. 회피 - 너무 가까우면 회피 (40% 확률)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 1.2f),
                new DefenderAI.CanDodge(this, blackboard),
                new DefenderAI.RandomChance(this, blackboard, 0.4f),
                new DefenderAI.DodgeAway(this, blackboard)
            ),

            // 5. 긴급 상황 - 가장자리에 몰렸으면 중앙으로 (전투보다 후순위)
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 2.0f), // 적이 가까이 있고
                new DefenderAI.MoveToOpenSpace(this, blackboard)   // 가장자리에 있으면 중앙으로
            ),

            // 6. 접근 - 너무 멀면 접근해서 공격 기회 만들기
            new SequenceNode(
                new DefenderAI.IsFarFromTarget(this, blackboard, 4.0f), // 4미터보다 멀면
                new DefenderAI.MoveToTarget(this, blackboard)
            ),

            // 7. 측면 이동 - 중거리에서 포지셔닝
            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.0f),
                new DefenderAI.MoveSideways(this, blackboard)
            ),

            // 8. 기본: 적절한 거리 유지하면서 기회 대기
            new DefenderAI.PatrolOrWait(this, blackboard)
        );

        SetRootNode(rootSelector);
    }
}