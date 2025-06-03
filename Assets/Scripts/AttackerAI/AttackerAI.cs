using UnityEngine;
using BehaviorTree;
public class AttackerAI : BehaviorTree.BehaviorTree
{
    [SerializeField] private Transform target;
    [SerializeField] private float followDistance = 3f; // 이 거리 이상 떨어지면 따라감
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;

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
        blackboard.SetValue("rotationSpeed", rotationSpeed);
        blackboard.SetValue("followDistance", followDistance);

        blackboard.SetValue("target", target);
    }

    protected override void OnUpdate()
    {
        blackboard.SetValue("target", target);
    }

    protected override void ConstructTree()
    {
        SelectorNode rootSelector = new SelectorNode();

        SequenceNode attackSequence = new SequenceNode();
        Inverter isTargetClose = new Inverter();
        isTargetClose.SetChild(new IsTargetTooFarNode(this, blackboard, followDistance));
        attackSequence.AddChild(isTargetClose);

        Repeat repeatAttack = new Repeat(3);
        repeatAttack.SetChild(new AttackPlayerNode(this, blackboard));
        attackSequence.AddChild(repeatAttack);

        SequenceNode moveSequence = new SequenceNode();
        moveSequence.AddChild(new IsTargetTooFarNode(this, blackboard, followDistance));
        moveSequence.AddChild(new MoveToTargetNode(this, blackboard, moveSpeed, followDistance * 0.8f));

        rootSelector.AddChild(attackSequence);
        rootSelector.AddChild(moveSequence);

        SetRootNode(rootSelector);
    }

}