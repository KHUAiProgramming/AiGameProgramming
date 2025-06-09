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

            new DefenderAI.CounterAfterBlock(this, blackboard),

            new SequenceNode(
                new DefenderAI.CanAttack(this, blackboard),
                new DefenderAI.IsInRange(this, blackboard, 1.3f),
                new ProbabilitySelector(0.3f,
                    new DefenderAI.AttackAction(this, blackboard),
                    new ProbabilitySelector(0.7f,
                        new DefenderAI.BlockAction(this, blackboard),
                        new DefenderAI.DodgeRandom(this, blackboard)
                    )
                )
            ),

            new SequenceNode(
                new DefenderAI.IsInRange(this, blackboard, 3.0f),
                new DefenderAI.MoveSideways(this, blackboard)
            ),

            new DefenderAI.PatrolOrWait(this, blackboard, 3.5f, 0.6f, 0.3f),
            new DefenderAI.MoveToOpenSpace(this, blackboard)
            
        );

        SetRootNode(rootSelector);
    }
}