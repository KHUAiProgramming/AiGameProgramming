using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

public class DefenderAI : BehaviorTree.BehaviorTree
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
        blackboard.SetValue("controller", GetComponent<DefenderController>());
        blackboard.SetValue("target", target);
    }

    protected override void OnUpdate()
    {
        blackboard.SetValue("target", target);
    }

    protected override void ConstructTree()
    {
        // 행동 노드들
        var isEnemyNear = new IsEnemyNear(this, blackboard);
        var isEnemyFar = new IsEnemyFar(this, blackboard);
        var isEnemyInRange = new IsEnemyInRange(this, blackboard);
        var aim = new Aim(this, blackboard);
        var moveForward = new MoveForward(this, blackboard);     
        var moveBackward = new MoveBackward(this, blackboard);
        var moveLeft = new MoveLeft(this, blackboard);
        var moveRight = new MoveRight(this, blackboard);
        var attack = new Attack(this, blackboard);
        var shield = new Shield(this, blackboard);
        var idle = new Idle(this, blackboard);

        // 전투 모드 트리 구성
        var farSequence = new SequenceNode(
            isEnemyFar,
            aim,
            moveBackward
        );

        var rangeAttackSequence = new SequenceNode(
            //isInRange,
            new RandomSelectorNode(attack, shield)
        );

        var combatRandomSelector = new RandomSelectorNode(attack, shield);

        var combatSequence = new SequenceNode(isEnemyInRange, combatRandomSelector);

        // var moveRandomSelector = new RandomSelectorNode(moveForward, moveBackward, moveLeft, moveRight);

        var backwardSequence = new SequenceNode(isEnemyNear, moveBackward);

        var forwardSequence = new SequenceNode(isEnemyFar, moveForward);

        var moveSelector = new SelectorNode(backwardSequence, forwardSequence);
        // 루트 셀렉터
        var rootSelector = new SequenceNode(aim, moveSelector, combatSequence);
        SetRootNode(rootSelector);
    }
}
