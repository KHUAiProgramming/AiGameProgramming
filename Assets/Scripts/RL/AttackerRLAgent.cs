using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AttackerRLAgent : Agent
{
    [Header("Training References")]
    [SerializeField] private AttackerController attackerController;
    [SerializeField] private DefenderController btDefender; // BT 수비형 상대
    [SerializeField] private Transform defenderTransform;

    [Header("Training Settings")]
    [SerializeField] private float maxEpisodeTime = 30f;
    [SerializeField] private float episodeTimer = 0f;

    [Header("Reward Settings")]
    [SerializeField] private float damageReward = 1.0f;
    [SerializeField] private float deathPenalty = -5.0f;
    [SerializeField] private float timeoutPenalty = -1.0f;
    [SerializeField] private float stunPenalty = -0.5f;
    [SerializeField] private float kickThroughDefenseReward = 2.0f;
    [SerializeField] private float normalAttackStunPenalty = -1.0f; // 일반공격으로 스턴당할 때

    private Vector3 initialAttackerPosition;
    private Vector3 initialDefenderPosition;
    private float previousAttackerHP;
    private float previousDefenderHP;
    private bool wasStunned = false;

    public override void Initialize()
    {
        // 컴포넌트 참조 확인
        if (attackerController == null)
            attackerController = GetComponent<AttackerController>();

        if (btDefender == null)
            btDefender = FindObjectOfType<DefenderController>();

        if (defenderTransform == null && btDefender != null)
            defenderTransform = btDefender.transform;

        // 초기 위치 저장
        initialAttackerPosition = transform.position;
        if (defenderTransform != null)
            initialDefenderPosition = defenderTransform.position;
    }

    public override void OnEpisodeBegin()
    {
        // 위치 초기화
        transform.position = initialAttackerPosition;
        if (defenderTransform != null)
            defenderTransform.position = initialDefenderPosition;

        // HP 초기화
        attackerController.ResetHP();
        btDefender.ResetHP();

        // 이전 HP 저장
        previousAttackerHP = attackerController.CurrentHP;
        previousDefenderHP = btDefender.CurrentHP;

        // 타이머 초기화
        episodeTimer = 0f;
        wasStunned = false;

        Debug.Log("AttackerRL Episode Started");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 자신의 상태 (8개)
        sensor.AddObservation(attackerController.CurrentHP / attackerController.MaxHP); // 정규화된 HP
        sensor.AddObservation(attackerController.IsAttacking ? 1f : 0f);
        sensor.AddObservation(attackerController.IsKickAttacking ? 1f : 0f);
        sensor.AddObservation(attackerController.IsBlocking ? 1f : 0f);
        sensor.AddObservation(attackerController.IsDodging ? 1f : 0f);
        sensor.AddObservation(attackerController.IsStunned ? 1f : 0f);
        sensor.AddObservation(attackerController.AttackCooldownRemaining / 3.0f); // 정규화
        sensor.AddObservation(attackerController.DodgeCooldownRemaining / 3.5f);

        // 상대방 상태 (8개)
        sensor.AddObservation(btDefender.CurrentHP / btDefender.MaxHP);
        sensor.AddObservation(btDefender.IsAttacking ? 1f : 0f);
        sensor.AddObservation(btDefender.IsBlocking ? 1f : 0f);
        sensor.AddObservation(btDefender.IsDodging ? 1f : 0f);
        sensor.AddObservation(btDefender.JustFinishedBlocking ? 1f : 0f);
        sensor.AddObservation(btDefender.AttackCooldownRemaining / 3.5f);
        sensor.AddObservation(btDefender.BlockCooldownRemaining / 2.5f);
        sensor.AddObservation(btDefender.DodgeCooldownRemaining / 4.0f);

        // 상대적 위치 및 거리 (4개)
        Vector3 relativePosition = defenderTransform.position - transform.position;
        sensor.AddObservation(relativePosition.x / 10f); // 정규화
        sensor.AddObservation(relativePosition.z / 10f);
        sensor.AddObservation(relativePosition.magnitude / 10f); // 거리
        sensor.AddObservation(Vector3.Dot(transform.forward, relativePosition.normalized)); // 방향

        // 총 20개 관찰값
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        episodeTimer += Time.fixedDeltaTime;

        // 연속 행동 (이동)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ);

        // 이동 실행
        if (moveDirection.magnitude > 0.1f)
        {
            attackerController.Move(moveDirection.normalized);
        }
        else
        {
            attackerController.Stop();
        }

        // 이산 행동 (전투 액션)
        int combatAction = actions.DiscreteActions[0];
        ExecuteCombatAction(combatAction);

        // 보상 계산
        CalculateRewards();

        // 에피소드 종료 조건 확인
        CheckEpisodeEnd();
    }

    private void ExecuteCombatAction(int action)
    {
        switch (action)
        {
            case 0: // 아무것도 하지 않음
                break;
            case 1: // 일반공격 (Attack)
                if (attackerController.CanAttack())
                {
                    attackerController.SetTarget(defenderTransform);
                    attackerController.Attack();
                }
                break;
            case 2: // 발차기 공격 (KickAttack)
                if (attackerController.CanAttack())
                {
                    attackerController.SetTarget(defenderTransform);
                    attackerController.KickAttack();
                }
                break;
            case 3: // 방어 (Block)
                if (attackerController.CanBlock())
                {
                    attackerController.Block();
                }
                break;
            case 4: // 회피 (Dodge)
                if (attackerController.CanDodge())
                {
                    Vector3 dodgeDirection = (transform.position - defenderTransform.position).normalized;
                    attackerController.Dodge(dodgeDirection);
                }
                break;
        }
    }

    private void CalculateRewards()
    {
        // HP 변화 기반 보상
        float attackerHPDelta = attackerController.CurrentHP - previousAttackerHP;
        float defenderHPDelta = btDefender.CurrentHP - previousDefenderHP;

        // 상대에게 데미지를 입혔을 때 보상
        if (defenderHPDelta < 0)
        {
            float damageDealt = Mathf.Abs(defenderHPDelta);
            AddReward(damageDealt * damageReward);
        }

        // 자신이 데미지를 받았을 때 페널티
        if (attackerHPDelta < 0)
        {
            float damageTaken = Mathf.Abs(attackerHPDelta);
            AddReward(-damageTaken * 0.5f);
        }

        // 스턴 상태 감지 및 페널티
        if (attackerController.IsStunned && !wasStunned)
        {
            // 방어형이 방어 중일 때 일반공격을 했다면 더 큰 페널티
            if (btDefender.IsBlocking)
            {
                AddReward(normalAttackStunPenalty);
            }
            else
            {
                AddReward(stunPenalty);
            }
            wasStunned = true;
        }
        else if (!attackerController.IsStunned)
        {
            wasStunned = false;
        }

        // 발차기로 방어 뚫기 성공 시 보상
        if (attackerController.IsKickAttacking && btDefender.IsBlocking)
        {
            AddReward(kickThroughDefenseReward);
        }

        // 거리 기반 보상 (너무 멀리 떨어져 있지 않도록)
        float distance = Vector3.Distance(transform.position, defenderTransform.position);
        if (distance > 5f)
        {
            AddReward(-0.001f);
        }

        // 시간 페널티 (너무 오래 걸리지 않도록)
        AddReward(-0.0005f);

        // 이전 HP 업데이트
        previousAttackerHP = attackerController.CurrentHP;
        previousDefenderHP = btDefender.CurrentHP;
    }

    private void CheckEpisodeEnd()
    {
        // 승리 조건: 상대 처치
        if (btDefender.IsDead)
        {
            AddReward(10f); // 승리 보상
            EndEpisode();
            return;
        }

        // 패배 조건: 자신 사망
        if (attackerController.IsDead)
        {
            AddReward(deathPenalty);
            EndEpisode();
            return;
        }

        // 시간 초과
        if (episodeTimer >= maxEpisodeTime)
        {
            AddReward(timeoutPenalty);
            EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 테스트용 수동 조작
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        // WASD로 이동
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");

        // 전투 액션
        discreteActions[0] = 0; // 기본값

        if (Input.GetKeyDown(KeyCode.J)) discreteActions[0] = 1; // 일반공격
        else if (Input.GetKeyDown(KeyCode.K)) discreteActions[0] = 2; // 발차기
        else if (Input.GetKey(KeyCode.Space)) discreteActions[0] = 3; // 방어
        else if (Input.GetKeyDown(KeyCode.LeftShift)) discreteActions[0] = 4; // 회피
    }
}