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
            new AttackerAI.IsDead(this, blackboard),
            new AttackerAI.IsStunned(this, blackboard),

            new SequenceNode(
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.AttackAction(this, blackboard)
            ),

            new SequenceNode(
                new AttackerAI.CanAttack(this, blackboard),
                new AttackerAI.KickAttackAction(this, blackboard)
            ),

            new SequenceNode(
                new AttackerAI.CanBlock(this, blackboard),
                new RandomDecorator(0.01f,
                    new AttackerAI.BlockAction(this, blackboard)
                )
            ),

            new AttackerAI.MoveToTarget(this, blackboard)
        );

        SetRootNode(rootSelector);
    }
}