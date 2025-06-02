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

    // void Update()
    // {
    //     base.Update();
    //     blackboard.SetValue("target", target);
    // }

    protected override void ConstructTree()
    {
        Debug.Log("construct");
        SelectorNode selector = new SelectorNode();

        SequenceNode sequence = new SequenceNode();
        sequence.AddChild(new IsTargetTooFarNode(this, blackboard, followDistance));
        sequence.AddChild(new MoveToTargetNode(this, blackboard, moveSpeed, followDistance * 0.8f));
        // followSequence.AddChild(new AttackPlayerNode(this, blackboard));
        RepeaterNode repeat1 = new RepeaterNode(3);
        repeat1.SetChild(new AttackPlayerNode(this, blackboard));

        sequence.AddChild(repeat1);

        selector.AddChild(sequence);

        SetRootNode(selector);
    }
}